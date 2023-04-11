﻿namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Spice86.Core.Emulator.InterruptHandlers;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.Callback;
using Spice86.Core.Emulator.Function;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

public class SystemBiosInt15Handler : InterruptHandler {
    private readonly ILoggerService _loggerService;
    
    public SystemBiosInt15Handler(Machine machine, ILoggerService loggerService) : base(machine) {
        _loggerService = loggerService;
        FillDispatchTable();
    }

    private void FillDispatchTable() {
        _dispatchTable.Add(0x6, new Callback(0x6, Unsupported));
        _dispatchTable.Add(0xC0, new Callback(0xC0, Unsupported));
        _dispatchTable.Add(0xC2, new Callback(0xC2, Unsupported));
        _dispatchTable.Add(0xC4, new Callback(0xC4, Unsupported));
        _dispatchTable.Add(0x88, new Callback(0x88, GetExtendedMemorySize));
    }

    public override byte Index => 0x15;

    public override void Run() {
        SegmentedAddress? csIp = _machine.Cpu.FunctionHandlerInUse.PeekReturnAddressOnMachineStack(CallType.INTERRUPT);
        if (csIp is not null) {
            _loggerService.LoggerPropertyBag.CodeSegment = csIp.Segment;
            _loggerService.LoggerPropertyBag.InstructionPointer = csIp.Offset;
        }
        byte operation = _state.AH;
        Run(operation);
    }

    /// <summary>
    /// Reports extended memory size in AX.
    /// </summary>
    public void GetExtendedMemorySize() {
        _state.AX = 0;
    }

    private void Unsupported() {
        // We are not an IBM PS/2
        SetCarryFlag(true, true);
        _state.AH = 0x86;
    }
}