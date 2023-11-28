namespace Spice86.Core.Emulator.InterruptHandlers.PlaceHolders;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Microsoft C Memory Overlay interrupt handler place holder (does nothing unless hooked)
/// </summary>
public class MemoryOverlayInterruptHandlerPlaceHolder : InterruptHandler {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public MemoryOverlayInterruptHandlerPlaceHolder(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }
    
    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x3f;
    
    /// <inheritdoc/>
    public override void Run() {
        //Exists only to be hooked by the program.
    }
}
