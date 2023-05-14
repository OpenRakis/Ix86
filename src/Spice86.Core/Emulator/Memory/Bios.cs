﻿namespace Spice86.Core.Emulator.Memory;

/// <summary>
/// Provides access to emulated memory mapped BIOS values.
/// </summary>
public sealed class Bios {
    private readonly Memory _memory;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    public Bios(Memory memory) {
        _memory = memory;
        VideoMode = 0x03;
        ScreenRows = 24;
        ScreenColumns = 80;
        CurrentVideoPage = 0;
        CharacterPointHeight = 16;
        CrtControllerBaseAddress = 0x03D4;
        DisplayCombinationCode = 0x08; // VGA with color monitor
    }

    /// <summary>
    /// Gets or sets the value of the disk motor timer.
    /// </summary>
    public byte DiskMotorTimer {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0040];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0040] = value;
    }

    /// <summary>
    /// Gets or sets the BIOS video mode.
    /// </summary>
    public byte VideoMode {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0049];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0049] = value;
    }

    /// <summary>
    /// Gets or sets the BIOS screen column count.
    /// </summary>
    public byte ScreenColumns {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x004A];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x004A] = value;
    }

    /// <summary>
    /// Gets or sets the CRT controller base address.
    /// </summary>
    public byte CurrentVideoPage {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0062];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0062] = value;
    }
    
    /// <summary>
    /// Gets or sets the CRT controller base address.
    /// </summary>
    public ushort CrtControllerBaseAddress {
        get => _memory.UInt16[MemoryMap.BiosDataSegment, 0x0063];
        set => _memory.UInt16[MemoryMap.BiosDataSegment, 0x0063] = value;
    }

    /// <summary>
    /// Gets or sets the current value of the real time clock.
    /// </summary>
    public uint RealTimeClock {
        get => _memory.UInt32[MemoryMap.BiosDataSegment, 0x006C];
        set => _memory.UInt32[MemoryMap.BiosDataSegment, 0x006C] = value;
    }

    /// <summary>
    /// Gets or sets the BIOS screen row count.
    /// </summary>
    public byte ScreenRows {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0084];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0084] = value;
    }

    /// <summary>
    /// Gets or sets the character point height.
    /// </summary>
    public ushort CharacterPointHeight {
        get => _memory.UInt16[MemoryMap.BiosDataSegment, 0x0085];
        set => _memory.UInt16[MemoryMap.BiosDataSegment, 0x0085] = value;
    }

    /// <summary>
    /// Gets or sets the BIOS video mode options.
    /// </summary>
    public byte VideoModeOptions {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0087];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0087] = value;
    }

    /// <summary>
    /// Gets or sets the EGA feature switch values.
    /// </summary>
    public byte FeatureSwitches {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0088];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0088] = value;
    }

    /// <summary>
    /// Gets or sets the video display data value.
    /// </summary>
    public byte VideoDisplayData {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0089];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x0089] = value;
    }
    
    /// <summary>
    /// Gets or sets the display combination code.
    /// </summary>
    public byte DisplayCombinationCode {
        get => _memory.UInt8[MemoryMap.BiosDataSegment, 0x008A];
        set => _memory.UInt8[MemoryMap.BiosDataSegment, 0x008A] = value;
    }
}