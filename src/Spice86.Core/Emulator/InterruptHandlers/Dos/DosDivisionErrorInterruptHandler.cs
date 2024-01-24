namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.CPU.Exceptions;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Emulates the DOS CPU division error interrupt handler, which terminates the current process.<br/>
/// <remarks>Ported from FreeDOS's 'entry.asm'</remarks>
/// </summary>
public class DosDivisionErrorInterruptHandler : InterruptHandler {
    private readonly DosInt21Handler _dosInt21h;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="dosInt21Handler">The DOS int21H handler.</param>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public DosDivisionErrorInterruptHandler(DosInt21Handler dosInt21Handler, IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
        _dosInt21h = dosInt21Handler;
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x0;

    /// <inheritdoc/>
    public override void Run() {
        _dosInt21h.PrintString("Interrupt divide by zero");
        State.IsRunning = false;
        throw new CpuDivisionErrorException("The CPU interrupt for divide by zero error was called.");
    }
}
