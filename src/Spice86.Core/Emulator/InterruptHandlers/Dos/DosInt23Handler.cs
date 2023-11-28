namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// DOS INT23H Handler. DOS Ctrl-C Exit Address.
/// </summary>
public class DosInt23Handler : InterruptHandler {
    private readonly DosInt22Handler _dosInt22Handler;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="dosInt22Handler">The DOS INT21H services.</param>
    /// <param name="memory">The emulator memory.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public DosInt23Handler(DosInt22Handler dosInt22Handler, IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
        _dosInt22Handler = dosInt22Handler;
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x23;
    
    /// <inheritdoc/>
    public override void Run() {
        //Abort program and return to DOS.
        _dosInt22Handler.Run();
    }
}
