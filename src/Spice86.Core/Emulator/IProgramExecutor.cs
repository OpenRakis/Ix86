namespace Spice86.Core.Emulator;

using Spice86.Core.Emulator.CPU;
using Spice86.Core.Emulator.Debugger;
using Spice86.Core.Emulator.Devices.Video;
using Spice86.Core.Emulator.VM;
using Spice86.Shared.Interfaces;

public interface IProgramExecutor : IDisposable, IDebuggableComponent {
    Machine Machine { get; }
    void Run();
    void DumpEmulatorStateToDirectory(string path);
    bool IsPaused { get; set; }
}