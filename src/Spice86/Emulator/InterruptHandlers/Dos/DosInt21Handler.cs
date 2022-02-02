﻿namespace Spice86.Emulator.InterruptHandlers.Dos;

using Serilog;

using Spice86.Emulator.Callback;
using Spice86.Emulator.Errors;
using Spice86.Emulator.VM;
using Spice86.Emulator.Memory;
using Spice86.Utils;

using System;
using System.IO;
using System.Text;

/// <summary>
/// Reimplementation of int21
/// </summary>
public class DosInt21Handler : InterruptHandler {
    private static readonly ILogger _logger = Log.Logger.ForContext<DosInt21Handler>();

    private readonly Encoding CP850_CHARSET;

    private DosMemoryManager _dosMemoryManager;
    private bool ctrlCFlag = false;

    // dosbox
    private byte defaultDrive = 2;

    private StringBuilder displayOutputBuilder = new StringBuilder();
    private DosFileManager dosFileManager;

    public DosInt21Handler(Machine machine) : base(machine) {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var westernIbmPcCodePage = Encoding.GetEncoding("ibm850");
        CP850_CHARSET = westernIbmPcCodePage;
        _dosMemoryManager = new DosMemoryManager(machine.GetMemory());
        dosFileManager = new DosFileManager(_memory);
        FillDispatchTable();
    }

    public void AllocateMemoryBlock(bool calledFromVm) {
        ushort requestedSize = _state.GetBX();
        _logger.Information("ALLOCATE MEMORY BLOCK {@RequestedSize}", requestedSize);
        SetCarryFlag(false, calledFromVm);
        DosMemoryControlBlock? res = _dosMemoryManager.AllocateMemoryBlock(requestedSize);
        if (res == null) {
            LogDosError(calledFromVm);
            // did not find something good, error
            SetCarryFlag(true, calledFromVm);
            DosMemoryControlBlock? largest = _dosMemoryManager.FindLargestFree();
            // INSUFFICIENT MEMORY
            _state.SetAX(0x08);
            if (largest != null) {
                _state.SetBX(largest.GetSize());
            } else {
                _state.SetBX(0);
            }
            return;
        }
        _state.SetAX(res.GetUsableSpaceSegment());
    }

    public void ChangeCurrentDirectory(bool calledFromVm) {
        String newDirectory = GetStringAtDsDx();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("SET CURRENT DIRECTORY: {@NewDirectory}", newDirectory);
        }
        DosFileOperationResult dosFileOperationResult = dosFileManager.SetCurrentDir(newDirectory);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void ClearKeyboardBufferAndInvokeKeyboardFunction() {
        byte operation = _state.GetAL();
        _logger.Debug("CLEAR KEYBOARD AND CALL INT 21 {@Operation}", operation);
        this.Run(operation);
    }

    public void CloseFile(bool calledFromVm) {
        ushort fileHandle = _state.GetBX();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("CLOSE FILE handle {@FileHandle}", ConvertUtils.ToHex(fileHandle));
        }
        DosFileOperationResult dosFileOperationResult = dosFileManager.CloseFile(fileHandle);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void CreateFileUsingHandle(bool calledFromVm) {
        String fileName = GetStringAtDsDx();
        ushort fileAttribute = _state.GetCX();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("CREATE FILE USING HANDLE: {@FileName} with attribute {@FileAttribute}", fileName, fileAttribute);
        }
        DosFileOperationResult dosFileOperationResult = dosFileManager.CreateFileUsingHandle(fileName, fileAttribute);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void directConsoleIo(bool calledFromVm) {
        byte character = _state.GetDL();
        if (character == 0xFF) {
            _logger.Debug("DIRECT CONSOLE IO, INPUT REQUESTED");
            // Read from STDIN, not implemented, return no character ready
            ushort? scancode = _machine.GetKeyboardInt16Handler().GetNextKeyCode();
            if (scancode == null) {
                SetZeroFlag(true, calledFromVm);
                _state.SetAL(0);
            } else {
                byte ascii = (byte)scancode.Value;
                SetZeroFlag(false, calledFromVm);
                _state.SetAL(ascii);
            }
        } else {
            // Output
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                _logger.Information("DIRECT CONSOLE IO, {@Character}, {@Ascii}", character, ConvertUtils.ToChar(character));
            }
        }
    }

    public void DirectConsoleIo(bool calledFromVm) {
        byte character = _state.GetDL();
        if (character == 0xFF) {
            _logger.Debug("DIRECT CONSOLE IO, INPUT REQUESTED");
            // Read from STDIN, not implemented, return no character ready
            ushort? scancode = _machine.GetKeyboardInt16Handler().GetNextKeyCode();
            if (scancode == null) {
                SetZeroFlag(true, calledFromVm);
                _state.SetAL(0);
            } else {
                byte ascii = (byte)scancode.Value;
                SetZeroFlag(false, calledFromVm);
                _state.SetAL(ascii);
            }
        } else {
            // Output
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                _logger.Information("DIRECT CONSOLE IO, {@Character}, {@Ascii}", character, ConvertUtils.ToChar(character));
            }
        }
    }

    public void DiskReset() {
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("DISK RESET (Nothing to do...)");
        }
    }

    public void DisplayOutput() {
        byte characterByte = _state.GetDL();
        String character = ConvertDosChar(characterByte);
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("PRINT CHR: {@CharacterByte} ({@Character})", ConvertUtils.ToHex8(characterByte), character);
        }
        if (characterByte == '\r') {
            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                _logger.Information("PRINT CHR LINE BREAK: {@DisplayOutputBuilder}", displayOutputBuilder);
            }
            displayOutputBuilder = new StringBuilder();
        } else if (characterByte != '\n') {
            displayOutputBuilder.Append(character);
        }
    }

    public void DuplicateFileHandle(bool calledFromVm) {
        ushort fileHandle = _state.GetBX();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("DUPLICATE FILE HANDLE. {@FileHandle}", fileHandle);
        }
        DosFileOperationResult dosFileOperationResult = dosFileManager.DuplicateFileHandle(fileHandle);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void FindFirstMatchingFile(bool calledFromVm) {
        ushort attributes = _state.GetCX();
        String fileSpec = GetStringAtDsDx();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("FIND FIRST MATCHING FILE {@Attributes}, {@FileSpec}", ConvertUtils.ToHex16(attributes), fileSpec);
        }
        DosFileOperationResult dosFileOperationResult =
            dosFileManager.FindFirstMatchingFile(fileSpec);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void FindNextMatchingFile(bool calledFromVm) {
        ushort attributes = _state.GetCX();
        String fileSpec = GetStringAtDsDx();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("FIND NEXT MATCHING FILE {@Attributes}, {@FileSpec}", ConvertUtils.ToHex16(attributes), fileSpec);
        }
        DosFileOperationResult dosFileOperationResult =
            dosFileManager.FindNextMatchingFile();
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void FreeMemoryBlock(bool calledFromVm) {
        ushort blockSegment = _state.GetES();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("FREE ALLOCATED MEMORY {@BlockSegment}", blockSegment);
        }
        SetCarryFlag(false, calledFromVm);
        if (!_dosMemoryManager.FreeMemoryBlock((ushort)(blockSegment - 1))) {
            LogDosError(calledFromVm);
            SetCarryFlag(true, calledFromVm);
            // INVALID MEMORY BLOCK ADDRESS
            _state.SetAX(0x09);
        }
    }

    public void GetCurrentDefaultDrive() {
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET CURRENT DEFAULT DRIVE");
        }
        _state.SetAL(defaultDrive);
    }

    public void GetDate() {
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET DATE");
        }
        DateTime now = DateTime.Now;
        _state.SetAL((byte)now.DayOfWeek);
        _state.SetCX((ushort)now.Year);
        _state.SetDH((byte)now.Month);
        _state.SetDL((byte)now.Day);
    }

    public void GetDiskTransferAddress() {
        _state.SetES(dosFileManager.GetDiskTransferAreaAddressSegment());
        _state.SetBX(dosFileManager.GetDiskTransferAreaAddressOffset());
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET DTA (DISK TRANSFER ADDRESS) DS:DX {@DsDx}",
                ConvertUtils.ToSegmentedAddressRepresentation(_state.GetES(), _state.GetBX()));
        }
    }

    public void GetDosVersion() {
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET DOS VERSION");
        }
        // 5.0
        _state.SetAL(0x05);
        _state.SetAH(0x00);
        // FF => MS DOS
        _state.SetBH(0xFF);
        // DOS OEM KEY 0x00000
        _state.SetBL(0x00);
        _state.SetCX(0x00);
    }

    public void GetFreeDiskSpace() {
        byte driveNumber = _state.GetDL();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET FREE DISK SPACE FOR DRIVE {@DriveNumber}", driveNumber);
        }
        // 127 sectors per cluster
        _state.SetAX(127);
        // 512 bytes per sector
        _state.SetCX(512);
        // 4096 clusters available (~250MB)
        _state.SetBX(4096);
        // 8192 total clusters on disk (~500MB)
        _state.SetDX(8192);
    }

    public override byte GetIndex() {
        return 0x21;
    }

    public void GetInterruptVector() {
        byte vectorNumber = _state.GetAL();
        ushort segment = _memory.GetUint16((uint)(4 * vectorNumber + 2));
        ushort offset = _memory.GetUint16((uint)(4 * vectorNumber));
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET INTERRUPT VECTOR INT {@VectorInt}, got {@SegmentedAddress}", ConvertUtils.ToHex8(vectorNumber),
                ConvertUtils.ToSegmentedAddressRepresentation(segment, offset));
        }
        _state.SetES(segment);
        _state.SetBX(offset);
    }

    public void GetPspAddress() {
        ushort pspSegment = _dosMemoryManager.GetPspSegment();
        _state.SetBX(pspSegment);
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET PSP ADDRESS {@PspSegment}", ConvertUtils.ToHex16(pspSegment));
        }
    }

    public void GetSetControlBreak() {
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET/SET CTRL-C FLAG");
        }
        byte op = _state.GetAL();
        if (op == 0) {
            // GET
            _state.SetDL(ctrlCFlag ? (byte)1 : (byte)0);
        } else if (op == 1 || op == 2) {
            // SET
            ctrlCFlag = _state.GetDL() == 1;
        } else {
            throw new UnhandledOperationException(_machine, "Ctrl-C get/set operation unhandled: " + op);
        }
    }

    public void GetTime() {
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET TIME");
        }
        DateTime now = DateTime.Now;
        _state.SetCH((byte)now.Hour);
        _state.SetCL((byte)now.Minute);
        _state.SetDH((byte)now.Second);
        _state.SetDL((byte)now.Millisecond);
    }

    public void ModifyMemoryBlock(bool calledFromVm) {
        ushort requestedSize = _state.GetBX();
        ushort blockSegment = _state.GetES();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("MODIFY MEMORY BLOCK {@Size}, {@BlockSegment}", requestedSize, blockSegment);
        }
        SetCarryFlag(false, calledFromVm);
        if (!_dosMemoryManager.ModifyBlock((ushort)(blockSegment - 1), requestedSize)) {
            LogDosError(calledFromVm);
            // An error occurred. Report it as not enough memory.
            SetCarryFlag(true, calledFromVm);
            _state.SetAX(0x08);
            _state.SetBX(0);
        }
    }

    public void MoveFilePointerUsingHandle(bool calledFromVm) {
        byte originOfMove = _state.GetAL();
        ushort fileHandle = _state.GetBX();
        uint offset = (uint)(_state.GetCX() << 16 | _state.GetDX());
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("MOVE FILE POINTER USING HANDLE. {@OriginOfMove}, {@FileHandle}, {@Offset}", originOfMove, fileHandle,
            offset);
        }

        DosFileOperationResult dosFileOperationResult =
            dosFileManager.MoveFilePointerUsingHandle(originOfMove, fileHandle, offset);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void OpenFile(bool calledFromVm) {
        String fileName = GetStringAtDsDx();
        byte accessMode = _state.GetAL();
        byte rwAccessMode = (byte)(accessMode & 0b111);
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("OPEN FILE {@FileName} with mode {@AccessMod} (rwAccessMode:{@RwAccessMode})", fileName, ConvertUtils.ToHex8(accessMode),
                ConvertUtils.ToHex8(rwAccessMode));
        }
        DosFileOperationResult dosFileOperationResult = dosFileManager.OpenFile(fileName, rwAccessMode);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public void PrintString() {
        string str = GetDosString(_memory, _state.GetDS(), _state.GetDX(), '$');
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("PRINT STRING: {@String}", str);
        }
    }

    public void QuitWithExitCode() {
        byte exitCode = _state.GetAL();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("QUIT WITH EXIT CODE {@ExitCode}", ConvertUtils.ToHex8(exitCode));
        }
        _cpu.SetRunning(false);
    }

    public void ReadFile(bool calledFromVm) {
        ushort fileHandle = _state.GetBX();
        ushort readLength = _state.GetCX();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("READ FROM FILE handle {@FileHandle} length {@ReadLength} to {@DsDx}", fileHandle, readLength,
                ConvertUtils.ToSegmentedAddressRepresentation(_state.GetDS(), _state.GetDX()));
        }
        uint targetMemory = MemoryUtils.ToPhysicalAddress(_state.GetDS(), _state.GetDX());
        DosFileOperationResult dosFileOperationResult = dosFileManager.ReadFile(fileHandle, readLength, targetMemory);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    public override void Run() {
        byte operation = _state.GetAH();
        this.Run(operation);
    }

    public void SelectDefaultDrive() {
        defaultDrive = _state.GetDL();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("SELECT DEFAULT DRIVE {@DefaultDrive}", defaultDrive);
        }
        // Number of valid drive letters
        _state.SetAL(26);
    }

    public void SetDiskTransferAddress() {
        ushort segment = _state.GetDS();
        ushort offset = _state.GetDX();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("SET DTA (DISK TRANSFER ADDRESS) DS:DX {@DsDxSegmentOffset}",
                ConvertUtils.ToSegmentedAddressRepresentation(segment, offset));
        }
        dosFileManager.SetDiskTransferAreaAddress(segment, offset);
    }

    public void SetInterruptVector() {
        byte vectorNumber = _state.GetAL();
        ushort segment = _state.GetDS();
        ushort offset = _state.GetDX();
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("SET INTERRUPT VECTOR FOR INT {@VectorNumber} at address {@SegmentOffset}", ConvertUtils.ToHex(vectorNumber),
                ConvertUtils.ToSegmentedAddressRepresentation(segment, offset));
        }
        SetInterruptVector(vectorNumber, segment, offset);
    }

    public void SetInterruptVector(byte vectorNumber, ushort segment, ushort offset) {
        _memory.SetUint16((ushort)(4 * vectorNumber + 2), segment);
        _memory.SetUint16((ushort)(4 * vectorNumber), offset);
    }

    public void WriteFileUsingHandle(bool calledFromVm) {
        ushort fileHandle = _state.GetBX();
        ushort writeLength = _state.GetCX();
        uint bufferAddress = MemoryUtils.ToPhysicalAddress(_state.GetDS(), _state.GetDX());
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("WRITE TO FILE handle {@FileHandle} length {@WriteLength} from {@DsDx}", ConvertUtils.ToHex(fileHandle),
                ConvertUtils.ToHex(writeLength), ConvertUtils.ToSegmentedAddressRepresentation(_state.GetDS(), _state.GetDX()));
        }
        DosFileOperationResult dosFileOperationResult =
            dosFileManager.WriteFileUsingHandle(fileHandle, writeLength, bufferAddress);
        SetStateFromDosFileOperationResult(calledFromVm, dosFileOperationResult);
    }

    internal DosFileManager GetDosFileManager() {
        return this.dosFileManager;
    }

    internal DosMemoryManager GetDosMemoryManager() {
        return this._dosMemoryManager;
    }

    private string ConvertDosChar(byte characterByte) {
        return CP850_CHARSET.GetString(new [] { characterByte });
    }

    private void FillDispatchTable() {
        base._dispatchTable.Add(0x02, new Callback(0x02, DisplayOutput));
        base._dispatchTable.Add(0x06, new Callback(0x06, () => DirectConsoleIo(true)));
        base._dispatchTable.Add(0x09, new Callback(0x09, PrintString));
        base._dispatchTable.Add(0x0C, new Callback(0x0C, ClearKeyboardBufferAndInvokeKeyboardFunction));
        base._dispatchTable.Add(0x0D, new Callback(0x0D, DiskReset));
        base._dispatchTable.Add(0x0E, new Callback(0x0E, SelectDefaultDrive));
        base._dispatchTable.Add(0x1A, new Callback(0x1A, SetDiskTransferAddress));
        base._dispatchTable.Add(0x19, new Callback(0x19, GetCurrentDefaultDrive));
        base._dispatchTable.Add(0x25, new Callback(0x25, SetInterruptVector));
        base._dispatchTable.Add(0x2A, new Callback(0x2A, GetDate));
        base._dispatchTable.Add(0x2C, new Callback(0x2C, GetTime));
        base._dispatchTable.Add(0x2F, new Callback(0x2F, GetDiskTransferAddress));
        base._dispatchTable.Add(0x30, new Callback(0x30, GetDosVersion));
        base._dispatchTable.Add(0x33, new Callback(0x33, GetSetControlBreak));
        base._dispatchTable.Add(0x35, new Callback(0x35, GetInterruptVector));
        base._dispatchTable.Add(0x36, new Callback(0x36, () => GetFreeDiskSpace()));
        base._dispatchTable.Add(0x3B, new Callback(0x3B, () => ChangeCurrentDirectory(true)));
        base._dispatchTable.Add(0x3C, new Callback(0x3C, () => CreateFileUsingHandle(true)));
        base._dispatchTable.Add(0x3D, new Callback(0x3D, () => OpenFile(true)));
        base._dispatchTable.Add(0x3E, new Callback(0x3E, () => CloseFile(true)));
        base._dispatchTable.Add(0x3F, new Callback(0x3F, () => ReadFile(true)));
        base._dispatchTable.Add(0x40, new Callback(0x40, () => WriteFileUsingHandle(true)));
        base._dispatchTable.Add(0x43, new Callback(0x43, () => GetSetFileAttribute(true)));
        base._dispatchTable.Add(0x44, new Callback(0x44, () => IoControl(true)));
        base._dispatchTable.Add(0x42, new Callback(0x42, () => MoveFilePointerUsingHandle(true)));
        base._dispatchTable.Add(0x45, new Callback(0x45, () => DuplicateFileHandle(true)));
        base._dispatchTable.Add(0x47, new Callback(0x47, () => GetCurrentDirectory(true)));
        base._dispatchTable.Add(0x48, new Callback(0x48, () => AllocateMemoryBlock(true)));
        base._dispatchTable.Add(0x49, new Callback(0x49, () => FreeMemoryBlock(true)));
        base._dispatchTable.Add(0x4A, new Callback(0x4A, () => ModifyMemoryBlock(true)));
        base._dispatchTable.Add(0x4C, new Callback(0x4C, QuitWithExitCode));
        base._dispatchTable.Add(0x4E, new Callback(0x4E, () => FindFirstMatchingFile(true)));
        base._dispatchTable.Add(0x4F, new Callback(0x4F, () => FindNextMatchingFile(true)));
        base._dispatchTable.Add(0x62, new Callback(0x62, GetPspAddress));
    }

    private void GetCurrentDirectory(bool calledFromVm) {
        SetCarryFlag(false, calledFromVm);
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
            _logger.Information("GET CURRENT DIRECTORY {@ResponseAddress}",
                ConvertUtils.ToSegmentedAddressRepresentation(_state.GetDS(), _state.GetSI()));
        }
        uint responseAddress = MemoryUtils.ToPhysicalAddress(_state.GetDS(), _state.GetSI());
        // Fake that we are always at the root of the drive (empty String)
        _memory.SetUint8(responseAddress, 0);
    }

    private String GetDosString(Memory memory, ushort segment, ushort offset, char end) {
        uint stringStart = MemoryUtils.ToPhysicalAddress(segment, offset);
        StringBuilder stringBuilder = new StringBuilder();
        while (memory.GetUint8(stringStart) != end) {
            String c = ConvertDosChar(memory.GetUint8(stringStart++));
            stringBuilder.Append(c);
        }
        return stringBuilder.ToString();
    }

    private void GetSetFileAttribute(bool calledFromVm) {
        byte op = _state.GetAL();
        String fileName = GetStringAtDsDx();
        if (File.Exists(fileName)) {
            LogDosError(calledFromVm);
            SetCarryFlag(true, calledFromVm);
            // File not found
            _state.SetAX(0x2);
            return;
        }
        SetCarryFlag(false, calledFromVm);
        switch (op) {
            case 0: {
                    if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                        _logger.Information("GET FILE ATTRIBUTE {@FilneName}", fileName);
                    }
                    FileAttributes attributes = File.GetAttributes(fileName);
                    // let's always return the file is read / write
                    bool canWrite = (attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly;
                    _state.SetCX(canWrite ? (byte)0 : (byte)1);
                    break;
                }
            case 1: {
                    ushort attribute = _state.GetCX();
                    if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                        _logger.Information("SET FILE ATTRIBUTE {@FileName}, {@Attribute}", fileName, attribute);
                    }
                    break;
                }
            default: throw new UnhandledOperationException(_machine, "getSetFileAttribute operation unhandled: " + op);
        }
    }

    private String GetStringAtDsDx() {
        return GetDosString(_memory, _state.GetDS(), _state.GetDX(), '\0');
    }

    private void IoControl(bool calledFromVm) {
        byte op = _state.GetAL();
        ushort device = _state.GetBX();

        SetCarryFlag(false, calledFromVm);

        switch (op) {
            case 0: {
                    if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                        _logger.Information("GET DEVICE INFORMATION");
                    }
                    // Character or block device?
                    ushort res = device < DosFileManager.FileHandleOffset ? (ushort)0x80D3 : (ushort)0x02;
                    _state.SetDX(res);
                    break;
                }
            case 1: {
                    if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                        _logger.Information("SET DEVICE INFORMATION (unimplemented)");
                    }
                    break;
                }
            case 0xE: {
                    ushort driveNumber = _state.GetBL();
                    if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information)) {
                        _logger.Information("GET LOGICAL DRIVE FOR PHYSICAL DRIVE {@DriveNumber}", driveNumber);
                    }
                    // Only one drive
                    _state.SetAL(0);
                    break;
                }
            default: throw new UnhandledOperationException(_machine, $"IO Control operation unhandled: {op}");
        }
    }

    private void LogDosError(bool calledFromVm) {
        string returnMessage = "";
        if (calledFromVm) {
            returnMessage = $"Int will return to {_machine.PeekReturn()}. ";
        }
        if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Error)) {
            _logger.Error("DOS operation failed with an error. {@ReturnMessage}. State is {@State}", returnMessage, _state);
        }
    }

    private void SetStateFromDosFileOperationResult(bool calledFromVm, DosFileOperationResult dosFileOperationResult) {
        if (dosFileOperationResult.IsError()) {
            LogDosError(calledFromVm);
            SetCarryFlag(true, calledFromVm);
        } else {
            SetCarryFlag(false, calledFromVm);
        }
        uint? value = dosFileOperationResult.GetValue();
        if (value == null) {
            return;
        }
        _state.SetAX((ushort)value.Value);
        if (dosFileOperationResult.IsValueIsUint32()) {
            _state.SetDX((ushort)(value >> 16));
        }
    }
}