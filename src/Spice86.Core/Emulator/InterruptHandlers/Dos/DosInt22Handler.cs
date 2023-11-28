namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// FAR (DWORD) address of routine to be executed when program "returns to DOS". Should NEVER be called directly.
/// </summary>
public class DosInt22Handler : InterruptHandler {
    private readonly DosInt21Handler _dosInt21Handler;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="dosInt21Handler">The DOS INT21H services.</param>
    /// <param name="memory">The emulator memory.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public DosInt22Handler(DosInt21Handler dosInt21Handler, IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
        _dosInt21Handler = dosInt21Handler;
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x22;

    /// <inheritdoc/>
    public override void Run() {
        //We don't set the return code in AL, the program does it before exit.
        _dosInt21Handler.QuitWithExitCode();
    }
}
