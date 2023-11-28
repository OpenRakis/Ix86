namespace Spice86.Core.Emulator.VM;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.InterruptHandlers.PlaceHolders;
using Spice86.Core.Emulator.InterruptHandlers.PlaceHolders.FpuEmulation;
using Spice86.Core.Emulator.Memory;
using Spice86.Shared.Interfaces;

/// <summary>
/// Centralizes interrupt handlers only defined in the IDT so hooking them by the program can succeed.
/// </summary>
public class InterruptHandlersPlaceHolders {

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="memory">The memory bus.</param>
    /// <param name="cpu">The emulated CPU.</param>
    /// <param name="loggerService">The logger service implementation</param>
    public InterruptHandlersPlaceHolders(IMemory memory, Cpu cpu, ILoggerService loggerService) {
        MemoryOverlayInterruptHandlerPlaceHolder = new(memory, cpu, loggerService);
        FpuEmulationInterrupt1 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt2 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt3 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt4 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt5 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt6 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt7 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt8 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt9 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt10 = new(memory, cpu, loggerService);
        FpuEmulationInterrupt11 = new(memory, cpu, loggerService);
    }

    /// <summary>
    /// Microsoft C memory overlay interrupt handler place holder
    /// </summary>
    public MemoryOverlayInterruptHandlerPlaceHolder MemoryOverlayInterruptHandlerPlaceHolder { get; }

    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt1 FpuEmulationInterrupt1 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt2 FpuEmulationInterrupt2 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt3 FpuEmulationInterrupt3 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt4 FpuEmulationInterrupt4 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt5 FpuEmulationInterrupt5 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt6 FpuEmulationInterrupt6 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt7 FpuEmulationInterrupt7 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt8 FpuEmulationInterrupt8 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt9 FpuEmulationInterrupt9 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt10 FpuEmulationInterrupt10 { get; }
    /// <summary>
    /// FPU Emulation software intterrupt hooked by BORLAND C or other languge runtime.
    /// </summary>
    public FpuEmulationInterrupt11 FpuEmulationInterrupt11 { get; }
}
