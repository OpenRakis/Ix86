namespace Spice86.Core.CLI;

using CommandLine;

using Spice86.Core.Emulator.Function;

/// <summary> Configuration for spice86, that is what to run and how. Set on startup. </summary>
public class Configuration {
    [Option('c', nameof(CDrive), Default = null, Required = false, HelpText = "Path to C drive, default is exe parent")]
    public string? CDrive { get; set; }

    [Option('e', nameof(Exe), Default = null, Required = false, HelpText = "Path to executable")]
    public string? Exe { get; set; }

    [Option('a', nameof(ExeArgs), Default = null, Required = false, HelpText = "List of parameters to give to the emulated program")]
    public string? ExeArgs { get; set; }

    [Option('x', nameof(ExpectedChecksum), Default = null, Required = false, HelpText = "Hexadecimal string representing the expected checksum of the emulated program")]
    public string? ExpectedChecksum { get; init; }

    public byte[] ExpectedChecksumValue { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Instantiated <see cref="OverrideSupplierClassName"/>. Created by <see cref="CommandLineParser"/>
    /// </summary>
    public IOverrideSupplier? OverrideSupplier { get; set; }

    [Option('o', nameof(OverrideSupplierClassName), Default = null, Required = false, HelpText = "Name of a class in the current folder that will generate the initial function information. See documentation for more information.")]
    public string? OverrideSupplierClassName { get; init; }

    [Option('u', nameof(UseCodeOverride), Default = null, Required = false, HelpText = "<true or false> if false it will use the names provided by overrideSupplierClassName but not the code")]
    public bool? UseCodeOverride { get; set; }

    /// <summary>
    /// Use this in the code for <see cref="UseCodeOverride"/>
    /// </summary>
    public bool UseCodeOverrideOption => UseCodeOverride ?? true;

    [Option('h', nameof(HeadlessMode), Default = false, Required = false, HelpText = "Headless mode. If true, no GUI is shown.")]
    public bool HeadlessMode { get; init; }

    [Option('d', nameof(DumpDataOnExit), Default = null, Required = false, HelpText = "When true, records data at runtime and dumps them at exit time")]
    public bool? DumpDataOnExit { get; set; }

    [Option('f', nameof(FailOnUnhandledPort), Default = false, Required = false, HelpText = "If true, will fail when encountering an unhandled IO port. Useful to check for unimplemented hardware. false by default.")]
    public bool FailOnUnhandledPort { get; init; }

    [Option('g', nameof(GdbPort), Default = null, Required = false, HelpText = "gdb port, if empty gdb server will not be created. If not empty, application will pause until gdb connects")]
    public int? GdbPort { get; init; }

    [Option('r', nameof(RecordedDataDirectory), Required = false, HelpText = "Directory to dump data to when not specified otherwise. If blank dumps to SPICE86_DUMPS_FOLDER, and if not defined dumps to working directory")]
    public string RecordedDataDirectory { get; init; } = Environment.GetEnvironmentVariable("SPICE86_DUMPS_FOLDER") ?? Environment.CurrentDirectory;

    [Option('v', nameof(InitializeDOS), Default = null, Required = false, HelpText = "Install DOS interrupt vectors or not")]
    public bool? InitializeDOS { get; set; }

    /// <summary>
    /// Only for <see cref="Emulator.Devices.Timer.Timer"/>
    /// </summary>
    [Option('i', nameof(InstructionsPerSecond), Required = false, HelpText = "<number of instructions that have to be executed by the emulator to consider a second passed> if blank will use time based timer.")]
    public long? InstructionsPerSecond { get; set; }

    [Option('t', nameof(TimeMultiplier), Default = 1, Required = false, HelpText = "<time multiplier> if >1 will go faster, if <1 will go slower.")]
    public double TimeMultiplier { get; init; }

    [Option('p', nameof(ProgramEntryPointSegment), Default = 0x1000, Required = false, HelpText = "Segment where to load the program. DOS PSP and MCB will be created before it.")]
    public int ProgramEntryPointSegment { get; init; }

    [Option('l', nameof(VerboseLogs), Default = false, Required = false, HelpText = "Enable verbose level logs")]
    public bool VerboseLogs { get; init; }

    [Option('w', nameof(WarningLogs), Default = false, Required = false, HelpText = "Enable warning level logs")]
    public bool WarningLogs { get; set; }

    [Option('m', nameof(Mt32RomsPath), Default = null, Required = false, HelpText = "Zip file or directory containing the MT-32 ROM files")]
    public string? Mt32RomsPath { get; init; }

    [Option('k', nameof(Kilobytes), Default = 8192, Required = false, HelpText = "The amount of main memory, in kilobytes")]
    public int Kilobytes { get; init; } = 8192;
}