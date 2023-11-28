namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.InterruptHandlers.Critical;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;


/// <summary>
/// An FPU error interrupt handler is redirected to INT 02 (Non Maskable Interrupt) by the BIOS
/// </summary>
public class BiosInt75FpuErrorHandler : InterruptHandler {
    private readonly NonMaskableInterruptHandler _nonMaskableInterruptHandler;
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="nmi">The Non Maskable Interrupt handler.</param>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public BiosInt75FpuErrorHandler(NonMaskableInterruptHandler nmi, IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
        _nonMaskableInterruptHandler = nmi;
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x75;

    /// <inheritdoc/>
    public override void Run() {
        _nonMaskableInterruptHandler.Run();
    }
}
