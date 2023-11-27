namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

using System;

/// <summary>
/// INT 1C System Timer tick called from INT 8
/// </summary>
public class SystemBiosInt1CHandler : InterruptHandler {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public SystemBiosInt1CHandler(IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
    }

    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x1C;

    /// <inheritdoc/>
    public override void Run() {
        //Exists only to be hooked by user programs.
        return;
    }
}
