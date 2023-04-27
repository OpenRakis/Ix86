namespace Spice86.Core.Emulator.InterruptHandlers.VGA;

public struct RegisterValueSet {
    private static readonly byte[] SequencerRegisterValueSet1 = {0x03, 0x08, 0x03, 0x00, 0x02};
    private static readonly byte[] SequencerRegisterValueSet2 = {0x03, 0x00, 0x03, 0x00, 0x02};
    private static readonly byte[] SequencerRegisterValueSet3 = {0x03, 0x09, 0x03, 0x00, 0x02};
    private static readonly byte[] SequencerRegisterValueSet4 = {0x03, 0x01, 0x01, 0x00, 0x06};
    private static readonly byte[] SequencerRegisterValueSet5 = {0x03, 0x09, 0x0f, 0x00, 0x06};
    private static readonly byte[] SequencerRegisterValueSet6 = {0x03, 0x01, 0x0f, 0x00, 0x06};
    private static readonly byte[] SequencerRegisterValueSet7 = {0x03, 0x01, 0x0f, 0x00, 0x0e};
    private static readonly byte[] CrtControllerRegisterValueSet1 = {0x2d, 0x27, 0x28, 0x90, 0x2b, 0xa0, 0xbf, 0x1f, 0x00, 0x4f, 0x0d, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x14, 0x1f, 0x96, 0xb9, 0xa3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet2 = {0x5f, 0x4f, 0x50, 0x82, 0x55, 0x81, 0xbf, 0x1f, 0x00, 0x4f, 0x0d, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x28, 0x1f, 0x96, 0xb9, 0xa3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet3 = {0x2d, 0x27, 0x28, 0x90, 0x2b, 0x80, 0xbf, 0x1f, 0x00, 0xc1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x14, 0x00, 0x96, 0xb9, 0xa2, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet4 = {0x5f, 0x4f, 0x50, 0x82, 0x54, 0x80, 0xbf, 0x1f, 0x00, 0xc1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x28, 0x00, 0x96, 0xb9, 0xc2, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet5 = {0x5f, 0x4f, 0x50, 0x82, 0x55, 0x81, 0xbf, 0x1f, 0x00, 0x4f, 0x0d, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x28, 0x0f, 0x96, 0xb9, 0xa3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet6 = {0x2d, 0x27, 0x28, 0x90, 0x2b, 0x80, 0xbf, 0x1f, 0x00, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x14, 0x00, 0x96, 0xb9, 0xe3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet7 = {0x5f, 0x4f, 0x50, 0x82, 0x54, 0x80, 0xbf, 0x1f, 0x00, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x28, 0x00, 0x96, 0xb9, 0xe3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet8 = {0x5f, 0x4f, 0x50, 0x82, 0x54, 0x80, 0xbf, 0x1f, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x83, 0x85, 0x5d, 0x28, 0x0f, 0x63, 0xba, 0xe3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet9 = {0x5f, 0x4f, 0x50, 0x82, 0x54, 0x80, 0x0b, 0x3e, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xea, 0x8c, 0xdf, 0x28, 0x00, 0xe7, 0x04, 0xe3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet10 = {0x5f, 0x4f, 0x50, 0x82, 0x54, 0x80, 0xbf, 0x1f, 0x00, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9c, 0x8e, 0x8f, 0x28, 0x40, 0x96, 0xb9, 0xa3, 0xff};
    private static readonly byte[] CrtControllerRegisterValueSet11 = {0x7f, 0x63, 0x63, 0x83, 0x6b, 0x1b, 0x72, 0xf0, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x59, 0x8d, 0x57, 0x32, 0x00, 0x57, 0x73, 0xe3, 0xff};
    private static readonly byte[] AttributeControllerRegisterValueSet1 = {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x14, 0x07, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x0c, 0x00, 0x0f, 0x08, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet2 = {0x00, 0x13, 0x15, 0x17, 0x02, 0x04, 0x06, 0x07, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x01, 0x00, 0x03, 0x00, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet3 = {0x00, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x01, 0x00, 0x01, 0x00, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet4 = {0x00, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x10, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x0e, 0x00, 0x0f, 0x08, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet5 = {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x01, 0x00, 0x0f, 0x00, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet6 = {0x00, 0x08, 0x00, 0x00, 0x18, 0x18, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet7 = {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x14, 0x07, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x01, 0x00, 0x0f, 0x00, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet8 = {0x00, 0x3f, 0x00, 0x3f, 0x00, 0x3f, 0x00, 0x3f, 0x00, 0x3f, 0x00, 0x3f, 0x00, 0x3f, 0x00, 0x3f, 0x01, 0x00, 0x0f, 0x00, 0x00};
    private static readonly byte[] AttributeControllerRegisterValueSet9 = {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x41, 0x00, 0x0f, 0x00, 0x00};
    private static readonly byte[] GraphicsControllerRegisterValueSet1 = {0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x0e, 0x0f, 0xff};
    private static readonly byte[] GraphicsControllerRegisterValueSet2 = {0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x0f, 0x0f, 0xff};
    private static readonly byte[] GraphicsControllerRegisterValueSet3 = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d, 0x0f, 0xff};
    private static readonly byte[] GraphicsControllerRegisterValueSet4 = {0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x0a, 0x0f, 0xff};
    private static readonly byte[] GraphicsControllerRegisterValueSet5 = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x0f, 0xff};
    private static readonly byte[] GraphicsControllerRegisterValueSet6 = {0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x05, 0x0f, 0xff};

    public static readonly Dictionary<int, VideoMode> VgaModes = new() {
        [0x00] = new VideoMode(new VgaMode(MemoryModel.Text, 40, 25, 4, 9, 16, VgaBios.ColorTextSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet1, 0x67, CrtControllerRegisterValueSet1, AttributeControllerRegisterValueSet1, GraphicsControllerRegisterValueSet1),
        [0x01] = new VideoMode(new VgaMode(MemoryModel.Text, 40, 25, 4, 9, 16, VgaBios.ColorTextSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet1, 0x67, CrtControllerRegisterValueSet1, AttributeControllerRegisterValueSet1, GraphicsControllerRegisterValueSet1),
        [0x02] = new VideoMode(new VgaMode(MemoryModel.Text, 80, 25, 4, 9, 16, VgaBios.ColorTextSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet2, 0x67, CrtControllerRegisterValueSet2, AttributeControllerRegisterValueSet1, GraphicsControllerRegisterValueSet1),
        [0x03] = new VideoMode(new VgaMode(MemoryModel.Text, 80, 25, 4, 9, 16, VgaBios.ColorTextSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet2, 0x67, CrtControllerRegisterValueSet2, AttributeControllerRegisterValueSet1, GraphicsControllerRegisterValueSet1),
        [0x04] = new VideoMode(new VgaMode(MemoryModel.Cga, 320, 200, 2, 8, 8, VgaBios.ColorTextSegment), 0xFF, Palettes.Cga, SequencerRegisterValueSet3, 0x63, CrtControllerRegisterValueSet3, AttributeControllerRegisterValueSet2, GraphicsControllerRegisterValueSet2),
        [0x05] = new VideoMode(new VgaMode(MemoryModel.Cga, 320, 200, 2, 8, 8, VgaBios.ColorTextSegment), 0xFF, Palettes.Cga, SequencerRegisterValueSet3, 0x63, CrtControllerRegisterValueSet3, AttributeControllerRegisterValueSet2, GraphicsControllerRegisterValueSet2),
        [0x06] = new VideoMode(new VgaMode(MemoryModel.Cga, 640, 200, 1, 8, 8, VgaBios.ColorTextSegment), 0xFF, Palettes.Cga, SequencerRegisterValueSet4, 0x63, CrtControllerRegisterValueSet4, AttributeControllerRegisterValueSet3, GraphicsControllerRegisterValueSet3),
        [0x07] = new VideoMode(new VgaMode(MemoryModel.Text, 80, 25, 4, 9, 16, VgaBios.MonochromeTextSegment), 0xFF, Palettes.Monochrome, SequencerRegisterValueSet2, 0x66, CrtControllerRegisterValueSet5, AttributeControllerRegisterValueSet4, GraphicsControllerRegisterValueSet4),
        [0x0D] = new VideoMode(new VgaMode(MemoryModel.Planar, 320, 200, 4, 8, 8, VgaBios.GraphicsSegment), 0xFF, Palettes.Cga, SequencerRegisterValueSet5, 0x63, CrtControllerRegisterValueSet6, AttributeControllerRegisterValueSet5, GraphicsControllerRegisterValueSet5),
        [0x0E] = new VideoMode(new VgaMode(MemoryModel.Planar, 640, 200, 4, 8, 8, VgaBios.GraphicsSegment), 0xFF, Palettes.Cga, SequencerRegisterValueSet6, 0x63, CrtControllerRegisterValueSet7, AttributeControllerRegisterValueSet5, GraphicsControllerRegisterValueSet5),
        [0x0F] = new VideoMode(new VgaMode(MemoryModel.Planar, 640, 350, 1, 8, 14, VgaBios.GraphicsSegment), 0xFF, Palettes.Monochrome, SequencerRegisterValueSet6, 0xa3, CrtControllerRegisterValueSet8, AttributeControllerRegisterValueSet6, GraphicsControllerRegisterValueSet5),
        [0x10] = new VideoMode(new VgaMode(MemoryModel.Planar, 640, 350, 4, 8, 14, VgaBios.GraphicsSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet6, 0xa3, CrtControllerRegisterValueSet8, AttributeControllerRegisterValueSet7, GraphicsControllerRegisterValueSet5),
        [0x11] = new VideoMode(new VgaMode(MemoryModel.Planar, 640, 480, 1, 8, 16, VgaBios.GraphicsSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet6, 0xe3, CrtControllerRegisterValueSet9, AttributeControllerRegisterValueSet8, GraphicsControllerRegisterValueSet5),
        [0x12] = new VideoMode(new VgaMode(MemoryModel.Planar, 640, 480, 4, 8, 16, VgaBios.GraphicsSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet6, 0xe3, CrtControllerRegisterValueSet9, AttributeControllerRegisterValueSet7, GraphicsControllerRegisterValueSet5),
        [0x13] = new VideoMode(new VgaMode(MemoryModel.Packed, 320, 200, 8, 8, 8, VgaBios.GraphicsSegment), 0xFF, Palettes.Vga, SequencerRegisterValueSet7, 0x63, CrtControllerRegisterValueSet10, AttributeControllerRegisterValueSet9, GraphicsControllerRegisterValueSet6),
        [0x6A] = new VideoMode(new VgaMode(MemoryModel.Planar, 800, 600, 4, 8, 16, VgaBios.GraphicsSegment), 0xFF, Palettes.Ega, SequencerRegisterValueSet6, 0xe3, CrtControllerRegisterValueSet11, AttributeControllerRegisterValueSet7, GraphicsControllerRegisterValueSet5)
    };
}