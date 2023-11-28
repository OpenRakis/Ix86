namespace Spice86.Core.Emulator.InterruptHandlers.Dos;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// The DOS Fatal Error Handler. Normally points to the resident portion of COMMAND.COM for unrecoverable I/O errors which displays Abort/Retry/Ignore
/// </summary>
public class DosInt24Handler : InterruptHandler {
    private readonly DosInt22Handler _dosInt22Handler;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="dosInt22Handler">The DOS INT21H services.</param>
    /// <param name="memory">The emulator memory.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>

    public DosInt24Handler(DosInt22Handler dosInt22Handler, IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
        _dosInt22Handler = dosInt22Handler;

    }
    /// <inheritdoc/>

    public override byte VectorNumber { get; } = 0x24;
/// <inheritdoc/>

    public override void Run() {
        //Not implemented, we just abort the program.
        _dosInt22Handler.Run();
    }
}
