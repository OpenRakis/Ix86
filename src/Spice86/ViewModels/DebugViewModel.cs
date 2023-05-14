namespace Spice86.ViewModels;

using Avalonia.Controls;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Core.Emulator.Devices.Video;
using Spice86.Core.Emulator.VM;
using Spice86.Models.Debugging;

public partial class DebugViewModel : ObservableObject {
    [ObservableProperty]
    private MachineInfo _machine = new();
    
    [ObservableProperty]
    private VideoCardInfo _videoCard = new();

    [ObservableProperty]
    private DateTime? _lastUpdate = null;

    private Machine? _emulatorMachine;

    private readonly DispatcherTimer _timer;
    
    public DebugViewModel() {
        if (!Design.IsDesignMode) {
            throw new InvalidOperationException("This constructor is not for runtime usage");
        }
        _timer = new DispatcherTimer();
    }

    [RelayCommand]
    public void UpdateData() => UpdateValues(this, EventArgs.Empty);
    
    public DebugViewModel(Machine machine) {
        _emulatorMachine = machine;
        _timer = new(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, UpdateValues);
        _timer.Start();
    }

    private void UpdateValues(object? sender, EventArgs e) {
        VideoState? videoState = _emulatorMachine?.VgaRegisters;
        if (videoState is null) {
            return;
        }

        VideoCard.DacReadIndex = videoState.DacRegisters.IndexRegisterReadMode;
        VideoCard.DacWriteIndex = videoState.DacRegisters.IndexRegisterWriteMode;
        VideoCard.DacPixelMask = videoState.DacRegisters.PixelMask;
        VideoCard.DacData = videoState.DacRegisters.DataPeek;

        VideoCard.AttributeControllerColorSelect = videoState.AttributeControllerRegisters.ColorSelectRegister.Value;
        VideoCard.AttributeControllerOverscanColor = videoState.AttributeControllerRegisters.OverscanColor;
        VideoCard.AttributeControllerAttributeModeControl = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.Value;
        VideoCard.AttributeControllerVideoOutput45Select = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.VideoOutput45Select;
        VideoCard.AttributeControllerPixelWidth8 = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.PixelWidth8;
        VideoCard.AttributeControllerPixelPanningCompatibility = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.PixelPanningCompatibility;
        VideoCard.AttributeControllerBlinkingEnabled = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.BlinkingEnabled;
        VideoCard.AttributeControllerLineGraphicsEnabled = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.LineGraphicsEnabled;
        VideoCard.AttributeControllerMonochromeEmulation = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.MonochromeEmulation;
        VideoCard.AttributeControllerGraphicsMode = videoState.AttributeControllerRegisters.AttributeControllerModeRegister.GraphicsMode;
        VideoCard.AttributeControllerColorPlaneEnable = videoState.AttributeControllerRegisters.ColorPlaneEnableRegister.Value;
        VideoCard.AttributeControllerHorizontalPixelPanning = videoState.AttributeControllerRegisters.HorizontalPixelPanning;

        VideoCard.CrtControllerAddressWrap = videoState.CrtControllerRegisters.CrtModeControlRegister.AddressWrap;
        VideoCard.CrtControllerBytePanning = videoState.CrtControllerRegisters.PresetRowScanRegister.BytePanning;
        VideoCard.CrtControllerByteWordMode = videoState.CrtControllerRegisters.CrtModeControlRegister.ByteWordMode;
        VideoCard.CrtControllerCharacterCellHeightRegister = videoState.CrtControllerRegisters.CharacterCellHeightRegister.Value;
        VideoCard.CrtControllerCharacterCellHeight = videoState.CrtControllerRegisters.CharacterCellHeightRegister.CharacterCellHeight;
        VideoCard.CrtControllerCompatibilityModeSupport = videoState.CrtControllerRegisters.CrtModeControlRegister.CompatibilityModeSupport;
        VideoCard.CrtControllerCompatibleRead = videoState.CrtControllerRegisters.HorizontalBlankingEndRegister.CompatibleRead;
        VideoCard.CrtControllerCountByFour = videoState.CrtControllerRegisters.UnderlineRowScanlineRegister.CountByFour;
        VideoCard.CrtControllerCountByTwo = videoState.CrtControllerRegisters.CrtModeControlRegister.CountByTwo;
        VideoCard.CrtControllerCrtcScanDouble = videoState.CrtControllerRegisters.CharacterCellHeightRegister.CrtcScanDouble;
        VideoCard.CrtControllerCrtModeControl = videoState.CrtControllerRegisters.CrtModeControlRegister.Value;
        VideoCard.CrtControllerCursorEnd = videoState.CrtControllerRegisters.TextCursorEndRegister.Value;
        VideoCard.CrtControllerCursorLocationHigh = videoState.CrtControllerRegisters.TextCursorLocationHigh;
        VideoCard.CrtControllerCursorLocationLow = videoState.CrtControllerRegisters.TextCursorLocationLow;
        VideoCard.CrtControllerCursorStart = videoState.CrtControllerRegisters.TextCursorStartRegister.Value;
        VideoCard.CrtControllerDisableTextCursor = videoState.CrtControllerRegisters.TextCursorStartRegister.DisableTextCursor;
        VideoCard.CrtControllerDisableVerticalInterrupt = videoState.CrtControllerRegisters.VerticalSyncEndRegister.DisableVerticalInterrupt;
        VideoCard.CrtControllerDisplayEnableSkew = videoState.CrtControllerRegisters.HorizontalBlankingEndRegister.DisplayEnableSkew;
        VideoCard.CrtControllerDoubleWordMode = videoState.CrtControllerRegisters.UnderlineRowScanlineRegister.DoubleWordMode;
        VideoCard.CrtControllerEndHorizontalBlanking = videoState.CrtControllerRegisters.HorizontalBlankingEndRegister.Value;
        VideoCard.CrtControllerEndHorizontalDisplay = videoState.CrtControllerRegisters.HorizontalDisplayEnd;
        VideoCard.CrtControllerEndHorizontalRetrace = videoState.CrtControllerRegisters.HorizontalSyncEndRegister.Value;
        VideoCard.CrtControllerEndVerticalBlanking = videoState.CrtControllerRegisters.VerticalBlankingEnd;
        VideoCard.CrtControllerHorizontalBlankingEnd = videoState.CrtControllerRegisters.HorizontalBlankingEndValue;
        VideoCard.CrtControllerHorizontalSyncDelay = videoState.CrtControllerRegisters.HorizontalSyncEndRegister.HorizontalSyncDelay;
        VideoCard.CrtControllerHorizontalSyncEnd = videoState.CrtControllerRegisters.HorizontalSyncEndRegister.HorizontalSyncEnd;
        VideoCard.CrtControllerHorizontalTotal = videoState.CrtControllerRegisters.HorizontalTotal;
        VideoCard.CrtControllerLineCompareRegister = videoState.CrtControllerRegisters.LineCompare;
        VideoCard.CrtControllerLineCompare = videoState.CrtControllerRegisters.LineCompareValue;
        VideoCard.CrtControllerOffset = videoState.CrtControllerRegisters.Offset;
        VideoCard.CrtControllerOverflow = videoState.CrtControllerRegisters.OverflowRegister.Value;
        VideoCard.CrtControllerPresetRowScan = videoState.CrtControllerRegisters.PresetRowScanRegister.PresetRowScan;
        VideoCard.CrtControllerPresetRowScanRegister = videoState.CrtControllerRegisters.PresetRowScanRegister.Value;
        VideoCard.CrtControllerRefreshCyclesPerScanline = videoState.CrtControllerRegisters.VerticalSyncEndRegister.RefreshCyclesPerScanline;
        VideoCard.CrtControllerSelectRowScanCounter = videoState.CrtControllerRegisters.CrtModeControlRegister.SelectRowScanCounter;
        VideoCard.CrtControllerStartAddress = videoState.CrtControllerRegisters.ScreenStartAddress;
        VideoCard.CrtControllerStartAddressHigh = videoState.CrtControllerRegisters.ScreenStartAddressHigh;
        VideoCard.CrtControllerStartAddressLow = videoState.CrtControllerRegisters.ScreenStartAddressLow;
        VideoCard.CrtControllerStartHorizontalBlanking = videoState.CrtControllerRegisters.HorizontalBlankingStart;
        VideoCard.CrtControllerStartHorizontalRetrace = videoState.CrtControllerRegisters.HorizontalSyncStart;
        VideoCard.CrtControllerStartVerticalBlanking = videoState.CrtControllerRegisters.HorizontalBlankingStart;
        VideoCard.CrtControllerTextCursorEnd = videoState.CrtControllerRegisters.TextCursorEndRegister.TextCursorEnd;
        VideoCard.CrtControllerTextCursorLocation = videoState.CrtControllerRegisters.TextCursorLocation;
        VideoCard.CrtControllerTextCursorSkew = videoState.CrtControllerRegisters.TextCursorEndRegister.TextCursorSkew;
        VideoCard.CrtControllerTextCursorStart = videoState.CrtControllerRegisters.TextCursorStartRegister.TextCursorStart;
        VideoCard.CrtControllerTimingEnable = videoState.CrtControllerRegisters.CrtModeControlRegister.TimingEnable;
        VideoCard.CrtControllerUnderlineLocation = videoState.CrtControllerRegisters.UnderlineRowScanlineRegister.Value;
        VideoCard.CrtControllerUnderlineScanline = videoState.CrtControllerRegisters.UnderlineRowScanlineRegister.UnderlineScanline;
        VideoCard.CrtControllerVerticalBlankingStart = videoState.CrtControllerRegisters.VerticalBlankingStartValue;
        VideoCard.CrtControllerVerticalDisplayEnd = videoState.CrtControllerRegisters.VerticalDisplayEndValue;
        VideoCard.CrtControllerVerticalDisplayEndRegister = videoState.CrtControllerRegisters.VerticalDisplayEnd;
        VideoCard.CrtControllerVerticalRetraceEnd = videoState.CrtControllerRegisters.VerticalSyncEndRegister.Value;
        VideoCard.CrtControllerVerticalRetraceStart = videoState.CrtControllerRegisters.VerticalSyncStart;
        VideoCard.CrtControllerVerticalSyncStart = videoState.CrtControllerRegisters.VerticalSyncStartValue;
        VideoCard.CrtControllerVerticalTimingHalved = videoState.CrtControllerRegisters.CrtModeControlRegister.VerticalTimingHalved;
        VideoCard.CrtControllerVerticalTotal = videoState.CrtControllerRegisters.VerticalTotalValue;
        VideoCard.CrtControllerVerticalTotalRegister = videoState.CrtControllerRegisters.VerticalTotal;
        VideoCard.CrtControllerWriteProtect = videoState.CrtControllerRegisters.VerticalSyncEndRegister.WriteProtect;
        
        VideoCard.GraphicsDataRotate = videoState.GraphicsControllerRegisters.DataRotateRegister.Value;
        VideoCard.GraphicsRotateCount = videoState.GraphicsControllerRegisters.DataRotateRegister.RotateCount;
        VideoCard.GraphicsFunctionSelect = videoState.GraphicsControllerRegisters.DataRotateRegister.FunctionSelect;
        VideoCard.GraphicsBitMask = videoState.GraphicsControllerRegisters.BitMask;
        VideoCard.GraphicsColorCompare = videoState.GraphicsControllerRegisters.ColorCompare;
        VideoCard.GraphicsReadMode = videoState.GraphicsControllerRegisters.GraphicsModeRegister.ReadMode;
        VideoCard.GraphicsWriteMode = videoState.GraphicsControllerRegisters.GraphicsModeRegister.WriteMode;
        VideoCard.GraphicsOddEven = videoState.GraphicsControllerRegisters.GraphicsModeRegister.OddEven;
        VideoCard.GraphicsShiftRegisterMode = videoState.GraphicsControllerRegisters.GraphicsModeRegister.ShiftRegisterMode;
        VideoCard.GraphicsIn256ColorMode = videoState.GraphicsControllerRegisters.GraphicsModeRegister.In256ColorMode;
        VideoCard.GraphicsModeRegister = videoState.GraphicsControllerRegisters.GraphicsModeRegister.Value;
        VideoCard.GraphicsMiscellaneousGraphics = videoState.GraphicsControllerRegisters.MiscellaneousGraphicsRegister.Value;
        VideoCard.GraphicsGraphicsMode = videoState.GraphicsControllerRegisters.MiscellaneousGraphicsRegister.GraphicsMode;
        VideoCard.GraphicsChainOddMapsToEven = videoState.GraphicsControllerRegisters.MiscellaneousGraphicsRegister.ChainOddMapsToEven;
        VideoCard.GraphicsMemoryMap = videoState.GraphicsControllerRegisters.MiscellaneousGraphicsRegister.MemoryMap;
        VideoCard.GraphicsReadMapSelect = videoState.GraphicsControllerRegisters.ReadMapSelectRegister.PlaneSelect;
        VideoCard.GraphicsSetReset = videoState.GraphicsControllerRegisters.SetReset.Value;
        VideoCard.GraphicsColorDontCare = videoState.GraphicsControllerRegisters.ColorDontCare;
        VideoCard.GraphicsEnableSetReset = videoState.GraphicsControllerRegisters.EnableSetReset.Value;

        VideoCard.SequencerResetRegister = videoState.SequencerRegisters.ResetRegister.Value;
        VideoCard.SequencerSynchronousReset = videoState.SequencerRegisters.ResetRegister.SynchronousReset;
        VideoCard.SequencerAsynchronousReset = videoState.SequencerRegisters.ResetRegister.AsynchronousReset;
        VideoCard.SequencerClockingModeRegister = videoState.SequencerRegisters.ClockingModeRegister.Value;
        VideoCard.SequencerDotsPerClock = videoState.SequencerRegisters.ClockingModeRegister.DotsPerClock;
        VideoCard.SequencerShiftLoad = videoState.SequencerRegisters.ClockingModeRegister.ShiftLoad;
        VideoCard.SequencerDotClock = videoState.SequencerRegisters.ClockingModeRegister.DotClock;
        VideoCard.SequencerShift4 = videoState.SequencerRegisters.ClockingModeRegister.Shift4;
        VideoCard.SequencerScreenOff = videoState.SequencerRegisters.ClockingModeRegister.ScreenOff;
        VideoCard.SequencerPlaneMask = videoState.SequencerRegisters.PlaneMaskRegister.Value;
        VideoCard.SequencerCharacterMapSelect = videoState.SequencerRegisters.CharacterMapSelectRegister.Value;
        VideoCard.SequencerCharacterMapA = videoState.SequencerRegisters.CharacterMapSelectRegister.CharacterMapA;
        VideoCard.SequencerCharacterMapB = videoState.SequencerRegisters.CharacterMapSelectRegister.CharacterMapB;
        VideoCard.SequencerSequencerMemoryMode = videoState.SequencerRegisters.MemoryModeRegister.Value;
        VideoCard.SequencerExtendedMemory = videoState.SequencerRegisters.MemoryModeRegister.ExtendedMemory;
        VideoCard.SequencerOddEvenMode = videoState.SequencerRegisters.MemoryModeRegister.OddEvenMode;
        VideoCard.SequencerChain4Mode = videoState.SequencerRegisters.MemoryModeRegister.Chain4Mode;

        LastUpdate = DateTime.Now;
    }
}