﻿namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Serilog;

using Spice86.Core.Emulator.InterruptHandlers;
using Spice86.Core.Emulator.VM;
using Spice86.Logging;

/// <summary>
/// Reimplementation of int20
/// </summary>
public class DosInt20Handler : InterruptHandler {
    private static readonly ILogger _logger = new Serilogger().Logger.ForContext<DosInt20Handler>();

    public DosInt20Handler(Machine machine) : base(machine) {
    }

    public override byte Index => 0x20;

    public override void Run() {
        _logger.Information("PROGRAM TERMINATE");
        _cpu.IsRunning = false;
    }
}