namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// The DOS INT28H Handler
/// </summary>
public class DosInt28Handler : InterruptHandler {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public DosInt28Handler(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x28;

    /// <inheritdoc/>
    public override void Run() {
        //Exists only to be hooked by user programs.
    }
}
