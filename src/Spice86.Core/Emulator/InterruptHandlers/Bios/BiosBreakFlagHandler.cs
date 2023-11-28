namespace Spice86.Core.Emulator.InterruptHandlers.Bios;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// The BIOS Break Flag Handler (INT1B)
/// </summary>
public class BiosBreakFlagHandler : InterruptHandler {
    private readonly BiosDataArea _dataArea;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="biosDataArea">The memory mappings of the global BIOS state.</param>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation.</param>
    public BiosBreakFlagHandler(BiosDataArea biosDataArea, IMemory memory, Cpu cpu, ILoggerService loggerService) : base(memory, cpu, loggerService) {
        _dataArea = biosDataArea;
    }
    
    /// <inheritdoc/>
    public override byte VectorNumber { get; } = 0x1B;

    /// <inheritdoc/>
    public override void Run() {
        _dataArea.BreakFlag = 1;
    }
}
