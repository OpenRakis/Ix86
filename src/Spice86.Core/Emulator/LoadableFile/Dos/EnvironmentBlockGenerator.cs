﻿namespace Spice86.Core.Emulator.LoadableFile.Dos;

using Spice86.Core.Emulator.VM;

public class EnvironmentBlockGenerator {
    private readonly Machine _machine;
    public EnvironmentBlockGenerator(Machine machine) => _machine = machine;


    /// <summary>
    /// Returns a byte array containing a process's environment block.
    /// </summary>
    /// <returns>Byte array containing the process's environment block.</returns>
    public byte[] BuildEnvironmentBlock() {
        byte[] environmentStrings = _machine.EnvironmentVariables.EnvironmentBlock;
        // Need 2 bytes between strings and path and a null terminator after path.
        byte[] fullBlock = new byte[environmentStrings.Length + 2];

        environmentStrings.CopyTo(fullBlock, 0);

        // Not sure what this is for.
        fullBlock[environmentStrings.Length] = 1;

        return fullBlock;
    }
}
