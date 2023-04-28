namespace Spice86.Core.Emulator.Devices.Video;

using Spice86.Aeon.Emulator.Video;
using Spice86.Aeon.Emulator.Video.Registers;
using Spice86.Core.Emulator.InterruptHandlers.VGA;

public class Renderer {
    public int Width { get; set; }
    public int Height { get; set; }
    public int BitsPerPixel { get; set; }
    public int Stride => BitsPerPixel * Width;
    public int Size => Stride * Height;

    public Renderer(IVideoState state, IVideoMemory memory) {
    }

}

public interface IVideoMemory {
}

public interface IVideoState {
    public DacRegisters DacRegisters { get; }
    public GeneralRegisters GeneralRegisters { get; }
    public SequencerRegisters SequencerRegisters { get; }
    public CrtControllerRegisters CrtControllerRegisters { get; }
    public GraphicsControllerRegisters GraphicsControllerRegisters { get; }
    public AttributeControllerRegisters AttributeControllerRegisters { get; }
    
    public VgaMode VideoMode { get; }
}
