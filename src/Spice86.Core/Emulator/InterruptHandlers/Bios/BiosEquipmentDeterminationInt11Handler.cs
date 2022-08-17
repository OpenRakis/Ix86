﻿namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Spice86.Core.Emulator.InterruptHandlers;
using Spice86.Core.Emulator.VM;

/// <summary>
/// Very basic implementation of int 11 that basically does nothing.
/// </summary>
public class BiosEquipmentDeterminationInt11Handler : InterruptHandler {

    public BiosEquipmentDeterminationInt11Handler(Machine machine) : base(machine) {
    }

    public override byte Index => 0x11;

    public override void Run() {
        _state.AX = 0;
    }
}