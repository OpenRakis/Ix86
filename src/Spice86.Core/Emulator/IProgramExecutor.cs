namespace Spice86.Core.Emulator;

/// <summary>
/// The program executor is responsible for starting and controlling the emulated program. <br/>
/// It loads and executes a program following the given configuration in the emulator.<br/>
/// Currently only supports DOS EXE and COM files.
/// </summary>
public interface IProgramExecutor : IDisposable {
    /// <summary>
    /// Starts the emulated program.
    /// </summary>
    void Run();

    /// <summary>
    /// Dumps the emulator state to the specified directory.
    /// </summary>
    /// <param name="path">The directory used for dumping the emulator state.</param>
    void DumpEmulatorStateToDirectory(string path);
}