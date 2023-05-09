﻿namespace Spice86.Core.Emulator.ReverseEngineer;

using Spice86.Core.Emulator.CPU;

using Spice86.Core.Emulator.VM;

/// <summary>
/// Represents a memory-based data structures that has a segmented base address. <br/>
/// That segmented address is stored in the DS register.
/// </summary>
public class MemoryBasedDataStructureWithDsBaseAddress : MemoryBasedDataStructureWithSegmentRegisterBaseAddress {
    
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="machine">The emulator machine.</param>
    public MemoryBasedDataStructureWithDsBaseAddress(Machine machine) : base(machine, SegmentRegisters.DsIndex) {
    }
}