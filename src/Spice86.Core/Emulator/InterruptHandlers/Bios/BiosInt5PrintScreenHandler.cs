namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Called when the 'Print Screen' key is used.
/// </summary>
public class BiosInt5PrintScreenHandler : InterruptHandler {

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public BiosInt5PrintScreenHandler(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }
    
    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x5;

    /// <inheritdoc/>
    public override void Run() {
        //Does nothing unless hooked.
    }
}
