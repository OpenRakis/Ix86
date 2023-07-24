﻿namespace Spice86.Core.Emulator.Gdb;

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Serilog.Events;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.Function.Dump;

using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.VM.Breakpoint;
using Spice86.Shared.Emulator.Memory;
using Spice86.Shared.Interfaces;
using Spice86.Shared.Utils;

/// <summary>
/// Handles custom GDB commands triggered in command line via the monitor prefix.<br/>
/// Custom commands list can be seen with the monitor help command.
/// </summary>
public class GdbCustomCommandsHandler {
    private readonly ILoggerService _loggerService;
    private readonly RecorderDataWriter _recordedDataWriter;
    private readonly GdbIo _gdbIo;
    private readonly Machine _machine;
    private readonly Action<BreakPoint> _onBreakpointReached;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="gdbIo">The GDB I/O handler.</param>
    /// <param name="machine">The emulator machine.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    /// <param name="onBreakpointReached">The action to invoke when the breakpoint is triggered.</param>
    /// <param name="recordedDataDirectory">The path were program execution data will be dumped, with the 'dumpAll' custom GDB command.</param>
    public GdbCustomCommandsHandler(GdbIo gdbIo, Machine machine, ILoggerService loggerService, Action<BreakPoint> onBreakpointReached,
        string recordedDataDirectory) {
        _loggerService = loggerService;
        _gdbIo = gdbIo;
        _machine = machine;
        _onBreakpointReached = onBreakpointReached;
        _recordedDataWriter = new RecorderDataWriter(recordedDataDirectory, machine, _loggerService);
    }

    
    /// <summary>
    /// Handles a custom command passed from GDB.
    /// </summary>
    /// <param name="command">The command string passed from GDB.</param>
    /// <returns>A response string to be sent back to GDB.</returns>
    public string HandleCustomCommands(string command) {
        string[] commandSplit = command.Split(",");
        if (commandSplit.Length != 2) {
            return _gdbIo.GenerateResponse("");
        }

        byte[] customHex = ConvertUtils.HexToByteArray(commandSplit[1]);
        string custom = Encoding.UTF8.GetString(customHex);
        string[] customSplit = custom.Split(" ");
        return ExecuteCustomCommand(customSplit);
    }

    private string BreakCycles(string[] args) {
        if (args.Length < 2) {
            return InvalidCommand("breakCycles can only work with one argument.");
        }

        string cyclesToWaitString = args[1];
        if (long.TryParse(cyclesToWaitString, out long cyclesToWait)) {
            long currentCycles = _machine.Cpu.State.Cycles;
            long cyclesBreak = currentCycles + cyclesToWait;
            AddressBreakPoint breakPoint = new AddressBreakPoint(BreakPointType.CYCLES, cyclesBreak, _onBreakpointReached, true);
            _machine.MachineBreakpoints.ToggleBreakPoint(breakPoint, true);
            if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                _loggerService.Debug("Breakpoint added for cycles!\n{@BreakPoint}", breakPoint);
            }

            return _gdbIo.GenerateMessageToDisplayResponse(
                $"Breakpoint added for cycles. Current cycles is {currentCycles}. Will wait for {cyclesToWait}. Will stop at {cyclesBreak}");
        }
        return InvalidCommand($"breakCycles argument needs to be a number. You gave {cyclesToWaitString}");
    }

    private string BreakCsIp(string[] args) {
        if (args.Length < 3) {
            return InvalidCommand("breakCsIp can only work with two arguments.");
        }
        try {
            uint cs = ConvertUtils.ParseHex32(args[1]);
            uint ip = ConvertUtils.ParseHex32(args[2]);
            AddressBreakPoint breakPoint = new AddressBreakPoint(BreakPointType.EXECUTION, MemoryUtils.ToPhysicalAddress((ushort)cs, (ushort)ip), _onBreakpointReached, false);
            _machine.MachineBreakpoints.ToggleBreakPoint(breakPoint, true);
            if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
                _loggerService.Debug("Breakpoint added for cs:ip!\n@{@BreakPoint}", breakPoint);
            }

            return _gdbIo.GenerateMessageToDisplayResponse(
                $"Breakpoint added for cs:ip. Current cs:ip is {_machine.Cpu.State.CS}:{_machine.Cpu.State.IpPhysicalAddress}. Will stop at {cs}:{ip}");
        } catch (FormatException) {
            return InvalidCommand($"breakCsIp arguments need to be two numbers. You gave {args[1]}:{args[2]}");
        }
    }

    private string BreakStop() {
        BreakPoint breakPoint = new UnconditionalBreakPoint(BreakPointType.MACHINE_STOP, _onBreakpointReached, false);
        _machine.MachineBreakpoints.ToggleBreakPoint(breakPoint, true);
        if (_loggerService.IsEnabled(LogEventLevel.Debug)) {
            _loggerService.Debug("Breakpoint added for end of execution!@\n{@BreakPoint}", breakPoint);
        }

        return _gdbIo.GenerateMessageToDisplayResponse("Breakpoint added for end of execution.");
    }

    private string CallStack() {
        return _gdbIo.GenerateMessageToDisplayResponse(DumpCallStack());
    }
    
    /// <summary>
    /// Returns a string that dumps the call stack.
    /// </summary>
    /// <returns>A string laying out the call stack.</returns>
    public string DumpCallStack() {
        FunctionHandler inUse = _machine.Cpu.FunctionHandlerInUse;
        StringBuilder sb = new();
        if (inUse.Equals(_machine.Cpu.FunctionHandlerInExternalInterrupt)) {
            sb.AppendLine("From external interrupt:");
        }

        sb.Append(inUse.DumpCallStack());
        return sb.ToString();
    }

    private string DumpAll() {
        try {
            _recordedDataWriter.DumpAll();
            return _gdbIo.GenerateMessageToDisplayResponse($"Dumped everything in {_recordedDataWriter.DumpDirectory}");
        } catch (IOException e) {
            e.Demystify();
            return _gdbIo.GenerateMessageToDisplayResponse(e.Message);
        }
    }

    private string ExecuteCustomCommand(params string[] args) {
        string originalCommand = args[0];
        string command = originalCommand.ToLowerInvariant();
        if (command.StartsWith("ram")) {
            return ReadRam(args);
        }

        return command switch {
            "help" => Help(""),
            "state" => State(),
            "breakstop" => BreakStop(),
            "callstack" => CallStack(),
            "peekret" => PeekRet(args),
            "dumpall" => DumpAll(),
            "breakcycles" => BreakCycles(args),
            "breakcsip" => BreakCsIp(args),
            "vbuffer" => Vbuffer(args),
            _ => InvalidCommand(originalCommand),
        };
    }

    private string ReadRam(string[] args) {
        if (args.Length != 2) {
            return Help($"ram command takes only one parameter which is the address in format segment:offset\n");
        }
        string command = args[0];
        string bitsString = command.Replace("ram", "");
        int bits;
        try {
            bits = int.Parse(bitsString);
        } catch (Exception e) {
            if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                _loggerService.Error(e, "{MethodName}", nameof(ReadRam));
            }
            return Help($"Unparseable bits value {bitsString}");
        }
        string addressString = args[1].ToUpper();
        string[] addressStringSplit = addressString.Split(":");
        string segmentString = addressStringSplit[0];
        ushort? segment = ExtractValueFromHexOrRegisterName(segmentString);
        if (segment == null) {
            return Help($"Invalid segment value ${segmentString}");
        }
        string offsetString = addressStringSplit[1];
        ushort? offset = ExtractValueFromHexOrRegisterName(offsetString);
        if (offset == null) {
            return Help($"Invalid offset value ${offsetString}");
        }
        uint physicalAddress = MemoryUtils.ToPhysicalAddress(segment.Value, offset.Value);
        IMemory memory = _machine.Memory;
        return bits switch {
            8 => _gdbIo.GenerateMessageToDisplayResponse(ConvertUtils.ToHex8(memory.UInt8[physicalAddress])),
            16 => _gdbIo.GenerateMessageToDisplayResponse(ConvertUtils.ToHex16(memory.UInt16[physicalAddress])),
            32 => _gdbIo.GenerateMessageToDisplayResponse(ConvertUtils.ToHex(memory.UInt32[physicalAddress])),
            _ => Help($"ram command needs to take a valid bit size. value {bits} is not supported."),
        };
    }

    private ushort? ExtractValueFromHexOrRegisterName(string valueOrRegisterName) {
        State state = _machine.Cpu.State;
        PropertyInfo? registerProperty = state.GetType().GetProperty(valueOrRegisterName);
        if (registerProperty != null) {
            return (ushort?)registerProperty.GetValue(state, null);
        }

        try {
            return ConvertUtils.ParseHex16(valueOrRegisterName);
        } catch (Exception e) {
            if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                _loggerService.Error(e, "{MethodName}", nameof(ExtractValueFromHexOrRegisterName));
            }
            return null;
        }
    }

    private static string ExtractAction(string[] args) {
        if (args.Length >= 2) {
            return args[1];
        }

        throw new ArgumentException("You need to specify an action. Valid actions are [refresh, add, remove]");
    }

    private string GetValidRetValues() {
        return string.Join(", ", Enum.GetNames(typeof(CallType)));
    }

    private string Help(string additionnalMessage) {
        return _gdbIo.GenerateMessageToDisplayResponse($@"{additionnalMessage}
Supported custom commands:
 -help: display this
 - dumpAll: dumps everything possible in the default directory which is {_recordedDataWriter.DumpDirectory}
 - breakCycles <number of cycles to wait before break>: breaks after the given number of cycles is reached
 - breakCsIp <number for CS, number for IP>: breaks once CS and IP match and before the instruction is executed
 - breakStop: setups a breakpoint when machine shuts down
 - callStack: dumps the callstack to see in which function you are in the VM.
 - peekRet<optional type>: displays the return address of the current function as stored in the stack in RAM.If a parameter is provided, dump the return on the stack as if the return was one of the provided type. Valid values are: {GetValidRetValues()}
 - state: displays the state of the machine
 - ramx: displays the content of ram at the specified segmented address with x being the number of bits to extract. Example: ram8 DS:1234 => Will display the byte at address DS:1234
 - vbuffer: family of commands to control video bufers:
   - vbuffer refresh: refreshes the screen
   - vbuffer add<address> <resolution> <scale?>: Example vbuffer add 0x1234 320x200 1.5 -> Add an additional buffer displaying what is at address 0x1234, with resolution 320x200 and scale 1.5
   - vbuffer remove <address>: Deletes the buffer at address
   - vbuffer list: Lists the buffers currently displayed
");
    }

    private string InvalidCommand(string command) {
        return Help($"Invalid command {command}\n");
    }

    private string PeekRet(string[] args) {
        if (args.Length == 1) {
            return _gdbIo.GenerateMessageToDisplayResponse(_machine.PeekReturn());
        } else {
            string returnType = args[1];
            bool parsed = Enum.TryParse(typeof(CallType), returnType, out object? callType);
            if (parsed == false) {
                return _gdbIo.GenerateMessageToDisplayResponse(
                    $"Could not understand {returnType} as a return type. Valid values are: {GetValidRetValues()}");
            }

            if (callType is CallType type) {
                return _gdbIo.GenerateMessageToDisplayResponse(PeekReturn(type));
            }
        }

        return "";
    }
    
    /// <summary>
    /// Peeks at the return address.
    /// </summary>
    /// <param name="returnCallType">The expected call type.</param>
    /// <returns>The return address string.</returns>
    public string PeekReturn(CallType returnCallType) {
        return SegmentedAddress.ToString(_machine.Cpu.FunctionHandlerInUse.PeekReturnAddressOnMachineStack(returnCallType));
    }

    private string State() {
        string state = _machine.Cpu.State.ToString();
        return _gdbIo.GenerateMessageToDisplayResponse(state);
    }

    private string Vbuffer(string[] args) {
        try {
            string action = ExtractAction(args);
            IGui? gui = _machine.Gui;

            // Actions for 1 parameter
            if ("refresh".Equals(action)) {
                gui?.UpdateScreen();
                return _gdbIo.GenerateResponse("");
            } else {
                return _gdbIo.GenerateMessageToDisplayResponse($"Could not understand action {action}");
            }
        } catch (ArgumentException e) {
            e.Demystify();
            return _gdbIo.GenerateMessageToDisplayResponse(e.Message);
        }
    }
}