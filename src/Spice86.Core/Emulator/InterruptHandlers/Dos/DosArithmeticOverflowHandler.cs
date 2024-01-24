namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

using System;

/// <summary>
/// DOS INT4 (Overflow flag) exception handler
/// </summary>
public class DosArithmeticOverflowHandler : InterruptHandler {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public DosArithmeticOverflowHandler(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x4;

    /// <inheritdoc/>
    public override void Run() {
        State.InterruptFlag = false;
    }
}
