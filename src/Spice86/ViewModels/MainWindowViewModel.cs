﻿namespace Spice86.ViewModels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Serilog.Events;

using Spice86.Core.CLI;
using Spice86.Core.Emulator;
using Spice86.Core.Emulator.Devices.Sound;
using Spice86.Core.Emulator.InternalDebugger;
using Spice86.Infrastructure;
using Spice86.Interfaces;
using Spice86.Shared.Diagnostics;
using Spice86.Shared.Emulator.Keyboard;
using Spice86.Shared.Emulator.Mouse;
using Spice86.Shared.Emulator.Video;
using Spice86.Shared.Interfaces;

using System.Threading;

using Key = Spice86.Shared.Emulator.Keyboard.Key;
using MouseButton = Spice86.Shared.Emulator.Mouse.MouseButton;
using Timer = System.Timers.Timer;

/// <inheritdoc cref="Spice86.Shared.Interfaces.IGui" />
public sealed partial class MainWindowViewModel : ViewModelBaseWithErrorDialog, IPauseStatus, IGui, IDisposable {
    private const double ScreenRefreshHz = 60;
    private readonly ILoggerService _loggerService;
    private readonly IHostStorageProvider _hostStorageProvider;
    private readonly IUIDispatcher _uiDispatcher;
    private readonly IProgramExecutorFactory _programExecutorFactory;
    private readonly IUIDispatcherTimerFactory _uiDispatcherTimerFactory;
    private readonly IAvaloniaKeyScanCodeConverter _avaloniaKeyScanCodeConverter;
    private readonly IWindowService _windowService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowInternalDebuggerCommand))]
    private bool _isProgramExecutorNotNull;

    private IProgramExecutor? _programExecutor;

    private IProgramExecutor? ProgramExecutor {
        get => _programExecutor;
        set {
            _programExecutor = value;
            Dispatcher.UIThread.Post(() => IsProgramExecutorNotNull = value is not null);
        }
    }

    private SoftwareMixer? _softwareMixer;
    private ITimeMultiplier? _pit;
    private DebugWindowViewModel? _debugViewModel;

    [ObservableProperty]
    private Configuration _configuration;
    
    private bool _disposed;
    private bool _renderingTimerInitialized;
    private Thread? _emulatorThread;
    private bool _isSettingResolution;
    private string _lastExecutableDirectory = string.Empty;
    private bool _closeAppOnEmulatorExit;
    private bool _isAppClosing;

    private static Action? _uiUpdateMethod;
    private readonly Timer _drawTimer = new(1000.0 / ScreenRefreshHz);
    private readonly SemaphoreSlim? _drawingSemaphoreSlim = new(1, 1);

    public event EventHandler<KeyboardEventArgs>? KeyUp;
    public event EventHandler<KeyboardEventArgs>? KeyDown;
    public event EventHandler<MouseMoveEventArgs>? MouseMoved;
    public event EventHandler<MouseButtonEventArgs>? MouseButtonDown;
    public event EventHandler<MouseButtonEventArgs>? MouseButtonUp;

    public MainWindowViewModel(IWindowService windowService, IAvaloniaKeyScanCodeConverter avaloniaKeyScanCodeConverter, IProgramExecutorFactory programExecutorFactory, IUIDispatcher uiDispatcher, IHostStorageProvider hostStorageProvider, ITextClipboard textClipboard, IUIDispatcherTimerFactory uiDispatcherTimerFactory, Configuration configuration, ILoggerService loggerService) : base(textClipboard) {
        _avaloniaKeyScanCodeConverter = avaloniaKeyScanCodeConverter;
        _windowService = windowService;
        Configuration = configuration;
        _programExecutorFactory = programExecutorFactory;
        _loggerService = loggerService;
        _hostStorageProvider = hostStorageProvider;
        _uiDispatcher = uiDispatcher;
        _uiDispatcherTimerFactory = uiDispatcherTimerFactory;
    }

    internal void OnMainWindowClosing() => _isAppClosing = true;

    internal void OnKeyUp(KeyEventArgs e) {
        KeyUp?.Invoke(this,
            new KeyboardEventArgs((Key)e.Key,
                false,
                _avaloniaKeyScanCodeConverter.GetKeyReleasedScancode((Key)e.Key),
                _avaloniaKeyScanCodeConverter.GetAsciiCode(
                    _avaloniaKeyScanCodeConverter.GetKeyReleasedScancode((Key)e.Key))));
    }

    [RelayCommand]
    public async Task SaveBitmap() {
        if (Bitmap is not null) {
            await _hostStorageProvider.SaveBitmapFile(Bitmap);
        }
    }
    
    private bool _showCursor;

    public bool ShowCursor {
        get => _showCursor;
        set {
            SetProperty(ref _showCursor, value);
            if (_showCursor) {
                Cursor?.Dispose();
                Cursor = Cursor.Default;
            } else {
                Cursor?.Dispose();
                Cursor = new Cursor(StandardCursorType.None);
            }
        }
    }

    private double _scale = 1;

    public double Scale {
        get => _scale;
        set => SetProperty(ref _scale, Math.Max(value, 1));
    }

    [ObservableProperty]
    private Cursor? _cursor = Cursor.Default;

    [ObservableProperty]
    private WriteableBitmap? _bitmap;

    internal void OnKeyDown(KeyEventArgs e) {
        KeyDown?.Invoke(this,
            new KeyboardEventArgs((Key)e.Key,
                true,
                _avaloniaKeyScanCodeConverter.GetKeyPressedScancode((Key)e.Key),
                _avaloniaKeyScanCodeConverter.GetAsciiCode(
                    _avaloniaKeyScanCodeConverter.GetKeyPressedScancode((Key)e.Key))));
    }

    [ObservableProperty]
    private string _statusMessage = "Emulator: not started.";

    [ObservableProperty]
    private string _asmOverrideStatus = "ASM Overrides: not used.";

    private bool _isPaused;
    
    public bool IsPaused {
        get => _isPaused;
        set {
            SetProperty(ref _isPaused, value);
            if (_softwareMixer is not null) {
                _softwareMixer.IsPaused = value;
            }
        }
    }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public void HideMouseCursor() => _uiDispatcher.Post(() => ShowCursor = false);

    public void ShowMouseCursor() => _uiDispatcher.Post(() => ShowCursor = true);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowPerformanceCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    [NotifyCanExecuteChangedFor(nameof(DumpEmulatorStateToFileCommand))]
    private bool _isEmulatorRunning;

    [RelayCommand(CanExecute = nameof(IsEmulatorRunning))]
    public async Task DumpEmulatorStateToFile() {
        if (ProgramExecutor is not null) {
            await _hostStorageProvider.DumpEmulatorStateToFile(Configuration, ProgramExecutor);
        }
    }

    [RelayCommand(CanExecute = nameof(IsEmulatorRunning))]
    public void Pause() {
        if (ProgramExecutor is null) {
            return;
        }
        IsPaused = ProgramExecutor.IsPaused = true;
    }

    [RelayCommand(CanExecute = nameof(IsEmulatorRunning))]
    public void Play() {
        if (ProgramExecutor is null) {
            return;
        }

        IsPaused = ProgramExecutor.IsPaused = false;
    }

    private void SetMainTitle() => MainTitle = $"{nameof(Spice86)} {Configuration.Exe}";

    [ObservableProperty]
    private string? _mainTitle;

    private double _timeMultiplier = 1;

    public double? TimeMultiplier {
        get => _timeMultiplier;
        set {
            if (value is not null) {
                SetProperty(ref _timeMultiplier, value.Value);
                _pit?.SetTimeMultiplier(value.Value);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(IsEmulatorRunning))]
    public void ShowPerformance() => IsPerformanceVisible = !IsPerformanceVisible;
    
    [RelayCommand]
    public void ResetTimeMultiplier() => TimeMultiplier = Configuration.TimeMultiplier;

    private void InitializeRenderingTimer() {
        if (_renderingTimerInitialized) {
            return;
        }
        _renderingTimerInitialized = true;
        _drawTimer.Elapsed += (_, _) => DrawScreen();
        _drawTimer.Start();
    }

    private void DrawScreen() {
        if (_disposed || _isSettingResolution || _isAppClosing || _uiUpdateMethod is null || Bitmap is null || RenderScreen is null) {
            return;
        }
        _drawingSemaphoreSlim?.Wait();
        try {
            using ILockedFramebuffer pixels = Bitmap.Lock();
            var uiRenderEventArgs = new UIRenderEventArgs(pixels.Address, pixels.RowBytes * pixels.Size.Height / 4);
            RenderScreen.Invoke(this, uiRenderEventArgs);
        } finally {
            if (!_disposed) {
                _drawingSemaphoreSlim?.Release();
            }
        }
        _uiDispatcher.Post(static () => _uiUpdateMethod.Invoke(), DispatcherPriority.Render);
    }

    public double MouseX { get; set; }
    public double MouseY { get; set; }

    public void OnMainWindowInitialized(Action uiUpdateMethod) {
        _uiUpdateMethod = uiUpdateMethod;
        if(RunEmulator()) {
            _closeAppOnEmulatorExit = true;
        }
    }

    private bool RunEmulator() {
        if (string.IsNullOrWhiteSpace(Configuration.Exe) ||
            string.IsNullOrWhiteSpace(Configuration.CDrive)) {
            return false;
        }
        _lastExecutableDirectory = Configuration.CDrive;
        StatusMessage = "Emulator starting...";
        AsmOverrideStatus = Configuration switch {
            { UseCodeOverrideOption: true, OverrideSupplier: not null } => "ASM code overrides: enabled.",
            { UseCodeOverride: false, OverrideSupplier: not null } =>
                "ASM code overrides: only functions names will be referenced.",
            _ => "ASM code overrides: none."
        };
        SetLogLevel(Configuration.SilencedLogs ? "Silent" : _loggerService.LogLevelSwitch.MinimumLevel.ToString());
        SetMainTitle();
        StartEmulatorThread();
        return true;
    }

    public void OnMouseButtonDown(PointerPressedEventArgs @event, Image image) {
        Avalonia.Input.MouseButton mouseButton = @event.GetCurrentPoint(image).Properties.PointerUpdateKind.GetMouseButton();
        MouseButtonDown?.Invoke(this, new MouseButtonEventArgs((MouseButton)mouseButton, true));
    }

    public void OnMouseButtonUp(PointerReleasedEventArgs @event, Image image) {
        Avalonia.Input.MouseButton mouseButton = @event.GetCurrentPoint(image).Properties.PointerUpdateKind.GetMouseButton();
        MouseButtonUp?.Invoke(this, new MouseButtonEventArgs((MouseButton)mouseButton, false));
    }

    public void OnMouseMoved(PointerEventArgs @event, Image image) {
        if (image.Source is null) {
            return;
        }
        MouseX = @event.GetPosition(image).X / image.Source.Size.Width;
        MouseY = @event.GetPosition(image).Y / image.Source.Size.Height;
        MouseMoved?.Invoke(this, new MouseMoveEventArgs(MouseX, MouseY));
    }

    public void SetResolution(int width, int height) => _uiDispatcher.Post(() => {
        _isSettingResolution = true;
        Scale = 1;
        if (Width != width || Height != height) {
            Width = width;
            Height = height;
            if (_disposed) {
                return;
            }
            _drawingSemaphoreSlim?.Wait();
            try {
                Bitmap?.Dispose();
                Bitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            } finally {
                if (!_disposed) {
                    _drawingSemaphoreSlim?.Release();
                }
            }
        }
        _isSettingResolution = false;
        InitializeRenderingTimer();
    }, DispatcherPriority.MaxValue);

    public event EventHandler<UIRenderEventArgs>? RenderScreen;

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (!_disposed) {
            _disposed = true;
            if (disposing) {
                _drawTimer.Stop();
                _drawTimer.Dispose();
                _uiDispatcher.Post(() => {
                    Bitmap?.Dispose();
                    Cursor?.Dispose();
                }, DispatcherPriority.MaxValue);
                _drawingSemaphoreSlim?.Dispose();
                PlayCommand.Execute(null);
                IsEmulatorRunning = false;
                DisposeEmulator();
                if (_emulatorThread?.IsAlive == true) {
                    _emulatorThread.Join();
                }
            }
        }
    }

    private void DisposeEmulator() => ProgramExecutor?.Dispose();

    private bool _isInitLogLevelSet;

    private string _currentLogLevel = "";

    public string CurrentLogLevel {
        get {
            if (_isInitLogLevelSet) {
                return _currentLogLevel;
            }
            SetLogLevel(_loggerService.AreLogsSilenced ? "Silent" : _loggerService.LogLevelSwitch.MinimumLevel.ToString());
            _isInitLogLevelSet = true;
            return _currentLogLevel;
        }
        set => SetProperty(ref _currentLogLevel, value);
    }

    [RelayCommand] public void SetLogLevelToSilent() => SetLogLevel("Silent");
    [RelayCommand] public void SetLogLevelToVerbose() => SetLogLevel("Verbose");
    [RelayCommand] public void SetLogLevelToDebug() => SetLogLevel("Debug");
    [RelayCommand] public void SetLogLevelToInformation() => SetLogLevel("Information");
    [RelayCommand] public void SetLogLevelToWarning() => SetLogLevel("Warning");
    [RelayCommand] public void SetLogLevelToError() => SetLogLevel("Error");
    [RelayCommand] public void SetLogLevelToFatal() => SetLogLevel("Fatal");

    private void SetLogLevel(string logLevel) {
        if (logLevel == "Silent") {
            CurrentLogLevel = logLevel;
            _loggerService.AreLogsSilenced = true;
            _loggerService.LogLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
        } else {
            _loggerService.AreLogsSilenced = false;
            _loggerService.LogLevelSwitch.MinimumLevel = Enum.Parse<LogEventLevel>(logLevel);
            CurrentLogLevel = logLevel;
        }
    }

    private void StartEmulatorThread() {
        _emulatorThread = new Thread(EmulatorThread) {
            Name = "Emulator"
        };
        _emulatorThread.Start();
    }

    private void OnEmulatorErrorOccured(Exception e) => _uiDispatcher.Post(() => {
        StatusMessage = "Emulator crashed.";
        ShowError(e);
    });

    private void EmulatorThread() {
        try {
            try {
                StartProgramExecutor();
            } catch (Exception e) {
                if (_loggerService.IsEnabled(LogEventLevel.Error)) {
                    _loggerService.Error(e, "An error occurred during execution");
                }
                OnEmulatorErrorOccured(e);
            }
        }  finally {
            _uiDispatcher.Post(() => IsEmulatorRunning = false);
            _uiDispatcher.Post(() => StatusMessage = "Emulator: stopped.");
            _uiDispatcher.Post(() => AsmOverrideStatus = "");
        }
    }

    [ObservableProperty]
    private PerformanceViewModel? _performanceViewModel;

    [ObservableProperty]
    private bool _isPerformanceVisible;

    [RelayCommand(CanExecute = nameof(IsProgramExecutorNotNull))]
    public async Task ShowInternalDebugger() {
        if (ProgramExecutor is not null) {
            _debugViewModel = new DebugWindowViewModel(_textClipboard, _hostStorageProvider, _uiDispatcherTimerFactory, this, ProgramExecutor);
            await _windowService.ShowDebugWindow(_debugViewModel);
        }
    }

    private void StartProgramExecutor() {
        (IProgramExecutor ProgramExecutor, SoftwareMixer? SoftwareMixer, ITimeMultiplier? Pit) viewModelEmulatorDependencies = CreateEmulator();
        ProgramExecutor = viewModelEmulatorDependencies.ProgramExecutor;
        _softwareMixer = viewModelEmulatorDependencies.SoftwareMixer;
        _pit = viewModelEmulatorDependencies.Pit;
        PerformanceViewModel = new(_uiDispatcherTimerFactory, ProgramExecutor, new PerformanceMeasurer(), this);
        _windowService.CloseDebugWindow();
        TimeMultiplier = Configuration.TimeMultiplier;
        _uiDispatcher.Post(() => IsEmulatorRunning = true);
        _uiDispatcher.Post(() => StatusMessage = "Emulator started.");
        ProgramExecutor?.Run();
        if (_closeAppOnEmulatorExit) {
            _uiDispatcher.Post(() => CloseMainWindow?.Invoke(this, EventArgs.Empty));
        }
    }
    
    private sealed class ViewModelEmulatorDependenciesVisitor : IInternalDebugger {
        public SoftwareMixer? SoftwareMixer { get; private set; }
        public ITimeMultiplier? Pit { get; private set; }

        public void Visit<T>(T component) where T : IDebuggableComponent {
            SoftwareMixer ??= component as SoftwareMixer;
            Pit ??= component as ITimeMultiplier;
        }
        public bool NeedsToVisitEmulator => SoftwareMixer is null || Pit is null;
    }

    private (IProgramExecutor ProgramExecutor, SoftwareMixer? SoftwareMixer, ITimeMultiplier? Pit) CreateEmulator() {
        IProgramExecutor programExecutor = _programExecutorFactory.Create(this);
        ViewModelEmulatorDependenciesVisitor visitor = new();
        programExecutor.Accept(visitor);
        return (programExecutor, visitor.SoftwareMixer, visitor.Pit);
    }
    public event EventHandler? CloseMainWindow;
}