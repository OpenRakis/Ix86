﻿namespace Spice86.Core.Emulator.Memory;
public class UInt16Indexer {
    private readonly Memory _memory;

    public UInt16Indexer(Memory memory) => _memory = memory;

    public ushort this[uint address] {
        get { return _memory.GetUint16(address); }
        set { _memory.SetUint16(address, value); }
    }

    public ushort this[ushort segment, ushort offset] {
        get { return this[MemoryUtils.ToPhysicalAddress(segment, offset)]; }
        set { this[MemoryUtils.ToPhysicalAddress(segment, offset)] = value; }
    }
}
