﻿namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Spice86.Core.Emulator.InterruptHandlers;
using Spice86.Core.Emulator.VM;
using Spice86.Core.Emulator.Callback;
using Spice86.Core.Emulator.Memory;

public class SystemBiosInt15Handler : InterruptHandler {
    public SystemBiosInt15Handler(Machine machine) : base(machine) {
        _dispatchTable.Add(0xC0, new Callback(0xC0, Unsupported));
        _dispatchTable.Add(0xC2, new Callback(0xC2, Unsupported));
        _dispatchTable.Add(0xC4, new Callback(0xC4, Unsupported));
        _dispatchTable.Add(0x87, new Callback(0x87, CopyExtendedMemory));
        _dispatchTable.Add(0x88, new Callback(0x88, GetExtendedMemorySize));
    }

    public override byte Index => 0x15;

    public override void Run() {
        byte operation = _state.AH;
        Run(operation);
    }

    /// <summary>
    /// No extended memory size present. Yet.
    /// Reports 0 in AX.
    /// </summary>
    public void GetExtendedMemorySize() {
        //We've got no extended memory (yet)
        _state.AX = 0;
    }

    public void CopyExtendedMemory() {
        bool enabled = _memory.IsA20Enabled;
        _machine.Memory.EnableOrDisableA20Gate(true);
        uint bytes = _state.ECX;
        uint data = _state.ESI;
        long source = _memory.UInt32[data + 0x12 ] & 0x00FFFFFF + _memory.UInt8[data + 0x16] << 24;
        long dest = _memory.UInt32[data + 0x1A] & 0x00FFFFFF + _memory.UInt8[data + 0x1E] << 24;
        _state.EAX = (_state.EAX & 0xFFFF) | (_state.EAX & 0xFFFF0000);
        _memory.MemCopy((uint)source, (uint)dest, bytes);
        _memory.EnableOrDisableA20Gate(enabled);
    }

    private void Unsupported() {
        // We are not an IBM PS/2
        SetCarryFlag(true, true);
        _state.AH = 0x86;
    }
}