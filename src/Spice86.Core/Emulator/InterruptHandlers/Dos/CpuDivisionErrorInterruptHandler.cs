namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.Exceptions;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Emulates the DOS CPU division error interrupt handler, which terminates the current process.<br/>
/// <remarks>Ported from FreeDOS's 'entry.asm'</remarks>
/// </summary>
public class CpuDivisionErrorInterruptHandler : InterruptHandler {

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public CpuDivisionErrorInterruptHandler(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {

    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x0;

    /// <inheritdoc/>
    public override void Run() {
        throw new CpuDivisionErrorException("The CPU interrupt for divide by zero error was called.");
    }
}
