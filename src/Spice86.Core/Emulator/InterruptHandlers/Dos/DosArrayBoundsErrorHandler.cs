namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// The interrupt handler called when the BOUND instruction threw a out of bounds hardware exception.
/// </summary>
public class DosArrayBoundsErrorHandler : InterruptHandler {

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>

    public DosArrayBoundsErrorHandler(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x6;

    /// <inheritdoc/>
    public override void Run() {
        _state.IsRunning = false;
    }
}
