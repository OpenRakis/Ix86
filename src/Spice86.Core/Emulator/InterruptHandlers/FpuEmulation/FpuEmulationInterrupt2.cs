namespace Spice86.Core.Emulator.InterruptHandlers.FpuEmulation;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
/// </summary>
public class FpuEmulationInterrupt2 : InterruptHandler {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation</param>

    public FpuEmulationInterrupt2(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x35;

    /// <inheritdoc/>
    public override void Run() {
        //Only exists to be hooked by the program.
    }
}
