﻿namespace Spice86.Core.Emulator.ReverseEngineer.DataStructure;

using Spice86.Core.Emulator.CPU.Registers;
using Spice86.Core.Emulator.VM;

/// <summary>
/// Represents a memory-based data structures that has a segmented base address. <br/>
/// That segmented address is stored in the CS register.
/// </summary>
public class MemoryBasedDataStructureWithCsBaseAddress : MemoryBasedDataStructureWithSegmentRegisterBaseAddress {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="machine">The emulator machine.</param>
    public MemoryBasedDataStructureWithCsBaseAddress(Machine machine) : base(machine, SegmentRegisters.CsIndex) {
    }
}