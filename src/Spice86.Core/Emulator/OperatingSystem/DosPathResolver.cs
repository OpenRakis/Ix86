namespace Spice86.Core.Emulator.OperatingSystem;

using System.IO;
using System.Linq;
using System.Text;

using Spice86.Core.Emulator.OperatingSystem.Enums;
using Spice86.Core.Emulator.OperatingSystem.Structures;
using Spice86.Shared.Utils;

/// <summary>
/// Translates DOS filepaths to host file paths, and vice-versa.
/// </summary>
internal class DosPathResolver {
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="configuration">The emulator configuration.</param>
    public DosPathResolver(Configuration configuration) {
        _driveMap = InitializeDriveMap(configuration);
        _currentDrive = 'C';
        SetCurrentDirValue(_currentDrive, _driveMap[_currentDrive].MountPoint);
    }

    private Dictionary<char, MountedFolder> _driveMap = new();

    /// <summary>
    /// The current DOS drive in use.
    /// </summary>
    private char _currentDrive;

    /// <summary>
    /// The full host path to the folder used by DOS as the current folder.
    /// </summary>
    public string CurrentHostDirectory => ConvertUtils.ToSlashPath(_driveMap[_currentDrive].FullName);

    /// <summary>
    /// Gets the current DOS directory.
    /// </summary>
    public DosFileOperationResult GetCurrentDosDirectory(byte driveNumber, out string currentDir) {
        //0 = default drive
        if (driveNumber == 0 && _driveMap.Any()) {
            MountedFolder mountedFolder = _driveMap[_currentDrive];
            currentDir = Path.GetRelativePath(mountedFolder.MountPoint, mountedFolder.CurrentDirectory).ToUpperInvariant();
            return DosFileOperationResult.NoValue();
        }
        else if (_driveMap.TryGetValue(DriveLetters[driveNumber - 1], out MountedFolder? mountedFolder)) {
            currentDir = Path.GetRelativePath(mountedFolder.MountPoint, mountedFolder.CurrentDirectory).ToUpperInvariant();
            return DosFileOperationResult.NoValue();
        }
        currentDir = "";
        return DosFileOperationResult.Error(ErrorCode.InvalidDrive);
    }

    private static string GetExeParentFolder(Configuration configuration) {
        string? exe = configuration.Exe;
        string fallbackValue = ConvertUtils.ToSlashFolderPath(Environment.CurrentDirectory);
        if (string.IsNullOrWhiteSpace(exe)) {
            return fallbackValue;
        }
        string? parent = Path.GetDirectoryName(exe);
        return string.IsNullOrWhiteSpace(parent) ? fallbackValue : ConvertUtils.ToSlashFolderPath(parent);
    }

    private Dictionary<char, MountedFolder> InitializeDriveMap(Configuration configuration) {
        string parentFolder = GetExeParentFolder(configuration);
        Dictionary<char, MountedFolder> driveMap = new();
        string? cDrive = configuration.CDrive;
        if (string.IsNullOrWhiteSpace(cDrive)) {
            cDrive = parentFolder;
        }
        cDrive = ConvertUtils.ToSlashFolderPath(cDrive);
        driveMap.Add('C', new MountedFolder('C', cDrive));
        return driveMap;
    }

    /// <summary>
    /// Create a relative path from the current host directory to another. Paths will be resolved before calculating the difference.
    /// Default path comparison for the active platform will be used (OrdinalIgnoreCase for Windows or Mac, Ordinal for Unix).
    /// </summary>
    /// <param name="hostPath">The destination path.</param>
    /// <returns>A string containing the relative host path, or <paramref name="hostPath"/> if the paths don't share the same root.</returns>
    public string GetRelativeHostPathToCurrentDirectory(string hostPath) => Path.GetRelativePath(CurrentHostDirectory, hostPath);

    private static bool IsWithinMountPoint(string hostFullPath, MountedFolder mountedFolder) =>
        hostFullPath.StartsWith(mountedFolder.MountPoint);

    /// <summary>
    /// Sets the current DOS folder.
    /// </summary>
    /// <param name="dosPath">The new DOS path to use as the current DOS folder.</param>
    /// <returns>A <see cref="DosFileOperationResult"/> that details the result of the operation.</returns>
    public DosFileOperationResult SetCurrentDir(string dosPath) {
        if (IsPathRooted(dosPath)) {
            string? hostPath = TryGetFullHostPathFromDos(dosPath);
            if (!string.IsNullOrWhiteSpace(hostPath)) {
                char driveLetter = StartsWithDosDrive(dosPath) ? dosPath[0] : _currentDrive;
                return SetCurrentDirValue(driveLetter, hostPath);
            } else {
                return DosFileOperationResult.Error(ErrorCode.PathNotFound);
            }
        }

        if (dosPath == "." || dosPath == @".\") {
            return DosFileOperationResult.NoValue();
        }

        if (dosPath == ".." || dosPath == @"..\") {
            string? newCurrentDir = Directory.GetParent(CurrentHostDirectory)?.FullName;
            return SetCurrentDirValue(_currentDrive, newCurrentDir);
        }

        while (dosPath.StartsWith("..\\")) {
            dosPath = dosPath[3..];
            string? newCurrentDir = Directory.GetParent(CurrentHostDirectory)?.FullName;
            SetCurrentDirValue(_currentDrive, newCurrentDir);
        }

        string? hostFullPath = TryGetFullHostPathFromDos(dosPath);
        return SetCurrentDirValue(_currentDrive, hostFullPath);
    }

    private DosFileOperationResult SetCurrentDirValue(char driveLetter, string? hostFullPath) {
        if (string.IsNullOrWhiteSpace(hostFullPath) ||
            !IsWithinMountPoint(hostFullPath, _driveMap[driveLetter]) ||
            Encoding.ASCII.GetByteCount(hostFullPath) > 255) {
            return DosFileOperationResult.Error(ErrorCode.PathNotFound);
        }

        _driveMap[driveLetter].CurrentDirectory = hostFullPath;
        return DosFileOperationResult.NoValue();
    }

    /// <summary>
    /// Converts the DOS path to a full host path of the parent directory.<br/>
    /// </summary>
    /// <param name="dosPath">The DOS path to convert.</param>
    /// <returns>A string containing the full path to the parent directory in the host file system, or <c>null</c> if nothing was found.</returns>
    public string? TryGetFullHostParentPathFromDos(string dosPath) {
        string? fullHostPath = TryGetFullHostPathFromDos(dosPath);
        if(string.IsNullOrWhiteSpace(fullHostPath)) {
            return null;
        }
        string? parent = Directory.GetParent(fullHostPath)?.FullName;
        if (string.IsNullOrWhiteSpace(parent)) {
            return null;
        }

        return ConvertUtils.ToSlashPath(parent);
    }

    private (string HostPrefixPath, string DosRelativePath) DeconstructDosPath(string dosPath) {
        if (IsPathRooted(dosPath)) {
            int length = 1;
            if (StartsWithDosDrive(dosPath)) {
                length = 3;
            }
            return (_driveMap[dosPath[0]].MountPoint, dosPath[length..]);
        } else if (StartsWithDosDrive(dosPath)) {
            return (_driveMap[dosPath[0]].MountPoint, dosPath[2..]);
        } else {
            return (_driveMap[_currentDrive].MountPoint, dosPath);
        }
    }

    /// <summary>
    /// Converts the DOS path to a full host path.<br/>
    /// </summary>
    /// <param name="dosPath">The DOS path to convert.</param>
    /// <returns>A string containing the full file path in the host file system, or <c>null</c> if nothing was found.</returns>

    public string? TryGetFullHostPathFromDos(string dosPath) {
        if(string.IsNullOrWhiteSpace(dosPath)) {
            return null;
        }
        (string HostPrefix, string DosRelativePath) = DeconstructDosPath(dosPath);

        if(string.IsNullOrWhiteSpace(DosRelativePath)) {
            return ConvertUtils.ToSlashPath(HostPrefix);
        }

        DirectoryInfo hostDirInfo = new DirectoryInfo(HostPrefix);

        string? relativeHostPath = hostDirInfo
            .EnumerateDirectories("*", new EnumerationOptions() {
                RecurseSubdirectories = true,
            })
            .Select(x => x.FullName)
            .Concat(
            hostDirInfo.EnumerateFiles("*",
            new EnumerationOptions() {
                RecurseSubdirectories = true,
            }).Select(x => x.FullName))
            .Select(x => x[HostPrefix.Length..])
            .OrderBy(x => x.Length)
            .FirstOrDefault(x => string.Equals(ConvertUtils.ToSlashPath(x), ConvertUtils.ToSlashPath(DosRelativePath), StringComparison.OrdinalIgnoreCase));

        if(string.IsNullOrWhiteSpace(relativeHostPath)) {
            return null;
        }

        return ConvertUtils.ToSlashPath(Path.Combine(HostPrefix, relativeHostPath));
    }

    /// <summary>
    /// Prefixes the given DOS path by either the mapped drive folder or the current host folder depending on whether there is a root in the path.<br/>
    /// Does not convert to a case sensitive path. <br/>
    /// Does not search for the file or folder on disk.
    /// </summary>
    /// <param name="dosPath">The DOS path to convert.</param>
    /// <returns>A string containing the combination of the host path and the DOS path.</returns>
    public string PrefixWithHostDirectory(string dosPath) {
        if (string.IsNullOrWhiteSpace(dosPath)) {
            return dosPath;
        }
        (string HostPrefix, string DosRelativePath) = DeconstructDosPath(dosPath);
        return ConvertUtils.ToSlashPath(Path.Combine(HostPrefix, DosRelativePath));
    }

    /// <summary>
    /// All the possible DOS drive letters
    /// </summary>
    private static char[] DriveLetters => new char[] {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};

    /// <summary>
    /// Gets or sets the <see cref="_currentDrive"/> with a byte value (0x0: A:, 0x1: B:, ...)
    /// </summary>
    public byte CurrentDriveIndex {
        get  => (byte)Array.IndexOf(DriveLetters, _currentDrive);
        set => _currentDrive = DriveLetters[value];
    }

    public byte NumberOfPotentiallyValidDriveLetters => (byte)_driveMap.Count;

    private bool StartsWithDosDrive(string path) =>
        path.Length >= 2 &&
        DriveLetters.Contains(char.ToUpperInvariant(path[0])) &&
        path[1] == ':';

    private bool IsPathRooted(string path) =>
        path.StartsWith(@"\") ||
        path.Length >= 3 &&
        StartsWithDosDrive(path) &&
        path[2] == '\\';

    /// <summary>
    /// Returns whether the folder or file name already exists, in DOS's case insensitive point of view.
    /// </summary>
    /// <param name="newFileOrDirectoryPath">The name of new file or folder we try to create.</param>
    /// <param name="hostFolder">The full path to the host folder to look into.</param>
    /// <returns>A boolean value indicating if there is any folder or file with the same name.</returns>
    public bool AnyDosDirectoryOrFileWithTheSameName(string newFileOrDirectoryPath, DirectoryInfo hostFolder) =>
        GetTopLevelDirsAndFiles(hostFolder.FullName).Any(x => string.Equals(x, newFileOrDirectoryPath, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<string> GetTopLevelDirsAndFiles(string hostPath, string searchPattern = "*") {
        return Directory
            .GetDirectories(hostPath, searchPattern)
            .Concat(Directory.GetFiles(hostPath, searchPattern));
    }
}