﻿namespace Spice86.Core.Emulator.LoadableFile;

using Serilog;

using System.IO;
using Spice86.Logging;
using Spice86.Core.Emulator.Memory;
using Spice86.Core.Utils;
using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.VM;

/// <summary>
/// Base class for loading executable files in the VM like exe, bios, ...
/// </summary>
public abstract class ExecutableFileLoader {
    protected Cpu _cpu;
    protected Machine _machine;
    protected Memory _memory;
    private static readonly ILogger _logger = new Serilogger().Logger.ForContext<ExecutableFileLoader>();

    protected ExecutableFileLoader(Machine machine) {
        _machine = machine;
        _cpu = machine.Cpu;
        _memory = machine.Memory;
    }

    public abstract byte[] LoadFile(string file, string? arguments);

    protected byte[] ReadFile(string file) {
        return File.ReadAllBytes(file);
    }

    protected void SetEntryPoint(ushort cs, ushort ip) {
        State state = _cpu.State;
        state.CS = cs;
        state.IP = ip;
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("Program entry point is {@ProgramEntry}", ConvertUtils.ToSegmentedAddressRepresentation(cs, ip));
        }
    }
}