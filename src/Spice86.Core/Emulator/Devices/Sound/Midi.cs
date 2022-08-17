﻿namespace Spice86.Core.Emulator.Devices.Sound;

using Spice86.Core.Emulator;
using Spice86.Core.Emulator.IOPorts;
using Spice86.Core.Emulator.Sound.Midi;
using Spice86.Core.Emulator.VM;

using System;

/// <summary>
/// MPU401 (Midi) implementation.
/// </summary>
public class Midi : DefaultIOPortHandler {
    private const int Command = 0x331;
    private const int Data = 0x330;

    private readonly GeneralMidi _generalMidi;

    public Midi(Machine machine, Configuration configuration) : base(machine, configuration) {
        _generalMidi = new GeneralMidi(configuration, configuration.Mt32RomsPath);
        _machine.Paused += Machine_Paused;
        _machine.Resumed += Machine_Resumed;
    }

    private void Machine_Resumed() {
        _generalMidi.Resume();
    }

    private void Machine_Paused() {
        _generalMidi.Pause();
    }

    public override byte ReadByte(int port) {
        return _generalMidi.ReadByte(port);
    }

    public override void InitPortHandlers(IOPortDispatcher ioPortDispatcher) {
        ioPortDispatcher.AddIOPortHandler(Data, this);
        ioPortDispatcher.AddIOPortHandler(Command, this);
    }

    public override void WriteByte(int port, byte value) {
        _generalMidi.WriteByte(port, value);
    }
}