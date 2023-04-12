﻿namespace Spice86.Core.Emulator.ReverseEngineer;

using Spice86.Core.Emulator.Memory;

using Spice86.Core.Emulator.Errors;
using Spice86.Shared.Emulator.Errors;

using System.Text;

public abstract class MemoryBasedDataStructureWithBaseAddressProvider : MemoryBasedDataStructure {
    protected MemoryBasedDataStructureWithBaseAddressProvider(Memory memory) : base(memory) {
    }

    public abstract uint BaseAddress { get; }

    public ushort GetUint16(int offset) {
        return GetUint16(BaseAddress, offset);
    }

    public Uint16Array GetUint16Array(int start, int length) {
        return GetUint16Array(BaseAddress, start, length);
    }

    public uint GetUint32(int offset) {
        return GetUint32(BaseAddress, offset);
    }

    public byte GetUint8(int offset) {
        return GetUint8(BaseAddress, offset);
    }

    public Uint8Array GetUint8Array(int start, int length) {
        return GetUint8Array(BaseAddress, start, length);
    }

    public string GetZeroTerminatedString(uint start, int maxLength) {
        return MemoryUtils.GetZeroTerminatedString(this.Memory.Ram, BaseAddress + start, maxLength);
    }

    public void SetUint16(int offset, ushort value) {
        SetUint16(BaseAddress, offset, value);
    }

    public void SetUint32(int offset, uint value) {
        SetUint32(BaseAddress, offset, value);
    }

    public void SetUint8(int offset, byte value) {
        SetUint8(BaseAddress, offset, value);
    }

    public void SetZeroTerminatedString(uint start, string value, int maxLength) {
        MemoryUtils.SetZeroTerminatedString(this.Memory.Ram, BaseAddress + start, value, maxLength);
    }
}