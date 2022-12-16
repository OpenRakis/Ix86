﻿using Serilog;
using Serilog.Events;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Emulator.VM;
using Spice86.Logging;

namespace Spice86.Core.Emulator.LoadableFile.Dos.Exe;

/// <summary>
/// Loads a DOS 16 bits EXE file in memory.
/// </summary>
public class ExeLoader : DosFileLoader {
    private static readonly ILogger _logger = Serilogger.Logger.ForContext<ExeLoader>();
    private readonly ushort _startSegment;

    public ExeLoader(Machine machine, ushort startSegment) : base(machine) {
        _startSegment = startSegment;
    }

    public override byte[] LoadFile(string file, string? arguments) {
        byte[] exe = ReadFile(file);
        if (_logger.IsEnabled(LogEventLevel.Debug)) {
            _logger.Debug("Exe size: {@ExeSize}", exe.Length);
        }
        ExeFile exeFile = new ExeFile(exe);
        if (_logger.IsEnabled(LogEventLevel.Debug)) {
            _logger.Debug("Read header: {@ReadHeader}", exeFile);
        }

        LoadExeFileInMemory(exeFile, _startSegment);
        ushort pspSegment = (ushort)(_startSegment - 0x10);
        SetupCpuForExe(exeFile, _startSegment, pspSegment);
        new PspGenerator(_machine).GeneratePsp(pspSegment, arguments);
        if (_logger.IsEnabled(LogEventLevel.Debug)) {
            _logger.Debug("Initial CPU State: {@CpuState}", _cpu.State);
        }
        return exe;
    }

    private void LoadExeFileInMemory(ExeFile exeFile, ushort startSegment) {
        uint physicalStartAddress = MemoryUtils.ToPhysicalAddress(startSegment, 0);
        _memory.LoadData(physicalStartAddress, exeFile.ProgramImage);
        for (int i = 0; i < exeFile.RelocationTable.Count; i++) {
            SegmentedAddress address = exeFile.RelocationTable[i];
            // Read value from memory, add the start segment offset and write back
            uint addressToEdit = MemoryUtils.ToPhysicalAddress(address.Segment, address.Offset) + physicalStartAddress;
            int segmentToRelocate = _memory.GetUint16(addressToEdit);
            segmentToRelocate += startSegment;
            _memory.SetUint16(addressToEdit, (ushort)segmentToRelocate);
        }
    }

    private void SetupCpuForExe(ExeFile exeFile, ushort startSegment, ushort pspSegment) {
        State state = _cpu.State;

        // MS-DOS uses the values in the file header to set the SP and SS registers and
        // adjusts the initial value of the SS register by adding the start-segment
        // address to it.
        state.SS = (ushort)(exeFile.InitSS + startSegment);
        state.SP = exeFile.InitSP;

        // Make DS and ES point to the PSP
        state.DS = pspSegment;
        state.ES = pspSegment;

        state.InterruptFlag = true;

        // Finally, MS-DOS reads the initial CS and IP values from the program's file
        // header, adjusts the CS register value by adding the start-segment address to
        // it, and transfers control to the program at the adjusted address.
        SetEntryPoint((ushort)(exeFile.InitCS + startSegment), exeFile.InitIP);
    }
}