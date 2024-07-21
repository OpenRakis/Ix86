namespace Spice86.Core.Emulator.CPU.CfgCpu.Linker;

using Spice86.Core.Emulator.CPU.CfgCpu.ControlFlowGraph;
using Spice86.Core.Emulator.InternalDebugger;

public class ExecutionContext : IDebuggableComponent {
    private static int _nextIndex = 0;

    public ExecutionContext() {
        Index = _nextIndex++;
    }

    public int Index { get; }
    /// <summary>
    /// Last node actually executed by the CPU
    /// </summary>
    public ICfgNode? LastExecuted { get; set; }
    
    /// <summary>
    /// Next node to execute according to the graph.
    /// </summary>
    public ICfgNode? NodeToExecuteNextAccordingToGraph { get; set; }

    //TODO: instead of visiting the execution context, inject the ExecutionContextManager in the UI.
    public void Accept<T>(T emulatorDebugger) where T : IInternalDebugger {
        emulatorDebugger.Visit(this);
    }
}