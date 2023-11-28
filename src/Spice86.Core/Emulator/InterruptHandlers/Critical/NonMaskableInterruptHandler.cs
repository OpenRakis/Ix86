namespace Spice86.Core.Emulator.InterruptHandlers.Critical;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Devices.ExternalInput;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

using System;

/// <summary>
/// INT2. An interrupt that cannot be ignored via software means
/// </summary>
public class NonMaskableInterruptHandler : InterruptHandler {
    private readonly DualPic _pic;
    /// <summary>
    /// Initializes a new isntance.
    /// </summary>
    /// <param name="pic">The Programmable Interrupt Controller.</param>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public NonMaskableInterruptHandler(DualPic pic, IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
        _pic = pic;
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x2;

    /// <inheritdoc/>
    public override void Run() {
        _pic.MaskAllInterrupts();
    }
}
