﻿namespace Spice86.Core.Emulator.Memory;

using Spice86.Core.Emulator.OperatingSystem.Devices;

/// <summary> Information about memory mapping of an IBM PC </summary>
public static class MemoryMap {
    /// <summary>
    /// Segment that contains a list of addresses of interrupt handlers.
    /// </summary>
    public const int InterruptVectorSegment = 0x0000;

    /// <summary>
    /// The length of the interrupt vector table.
    /// </summary>
    public const int InterruptVectorLength = 1024;

    /// <summary>
    /// Segment containing the BIOS data area.
    /// </summary>
    public const ushort BiosDataSegment = 0x0040;

    /// <summary>
    /// The start segment of the free memory area.
    /// </summary>
    public const int FreeMemoryStartSegment = 0x50;

    /// <summary>
    /// The length of the boot sector code.
    /// </summary>
    public const int BootSectorCodeLength = 512;

    /// <summary>
    /// Segment where the boot sector code is stored.
    /// </summary>
    public const int BootSectorCodeSegment = 0x07C0;

    /// <summary>
    /// Segment of the Extended BIOS Data Area.
    /// </summary>
    public const int ExtendedBiosDataAreaSegment = 0x9FC0;
    
    /// <summary>
    /// Segment of the graphic video memory.
    /// </summary>
    public const int GraphicVideoMemorySegment = 0xA000;

    /// <summary>
    /// Segment of the monochrome text video memory.
    /// </summary>
    public const int MonochromeTextVideoMemorySegment = 0xB000;

    /// <summary>
    /// Segment of the color text video memory.
    /// </summary>
    public const int ColorTextVideoMemorySegment = 0xB800;

    /// <summary>
    /// Segment where VGA BIOS is stored.
    /// </summary>
    public const ushort VideoBiosSegment = 0xC000;

    /// <summary>
    /// Segment where our interrupt handler routines are stored.
    /// These are filled with special opcode FE 38 XX CF which will make the CPU pass control to the registered
    /// callback with index 0xXX.
    /// The interrupt vector table at 0x0000:0x0000 will contain pointers to these routines.
    /// </summary>
    public const ushort InterruptHandlersSegment = 0xF000;

    /// <summary>
    /// Segment where DOS device driver headers are stored.
    /// <see cref="VirtualDeviceBase"/>
    /// </summary>
    public const ushort DeviceDriverSegment = 0xF800;
}