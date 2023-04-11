﻿namespace Spice86.Core.Emulator.Memory;

using Spice86.Shared.Utils;

public class UInt32Indexer {
    private readonly Memory _memory;

    public UInt32Indexer(Memory memory) => _memory = memory;

    public uint this[uint address] {
        get => _memory.GetUint32(address);
        set => _memory.SetUint32(address, value);
    }

    public uint this[ushort segment, ushort offset] {
        get => this[MemoryUtils.ToPhysicalAddress(segment, offset)];
        set => this[MemoryUtils.ToPhysicalAddress(segment, offset)] = value;
    }
}