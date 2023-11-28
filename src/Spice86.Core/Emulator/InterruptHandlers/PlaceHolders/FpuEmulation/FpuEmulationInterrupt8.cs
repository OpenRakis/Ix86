namespace Spice86.Core.Emulator.InterruptHandlers.PlaceHolders.FpuEmulation;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
/// </summary>
public class FpuEmulationInterrupt8 : InterruptHandler {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation</param>

    public FpuEmulationInterrupt8(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x3b;

    /// <inheritdoc/>
    public override void Run() {
        //Only exists to be hooked by the program.
    }
}
