﻿namespace Spice86.ViewModels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Spice86.Views;
using Spice86.Shared;
using Spice86.Shared.Interfaces;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <inheritdoc cref="Spice86.Shared.Interfaces.IVideoBufferViewModel" />
public sealed partial class VideoBufferViewModel : ObservableObject, IVideoBufferViewModel, IComparable<VideoBufferViewModel> {
    private bool _disposedValue;

    private Thread? _drawThread;

    private bool _exitDrawThread;

    private readonly ManualResetEvent _manualResetEvent = new(false);

    /// <summary>
    /// For AvaloniaUI Designer
    /// </summary>
    public VideoBufferViewModel() {
        if (Design.IsDesignMode == false) {
            throw new InvalidOperationException("This constructor is not for runtime usage");
        }
        Width = 320;
        Height = 200;
        Address = 1;
        _index = 1;
        Scale = 1;
        _frameRenderTimeWatch = new Stopwatch();
    }

    public VideoBufferViewModel(IVideoCard videoCard, double scale, int width, int height, uint address, int index, bool isPrimaryDisplay) {
        _videoCard = videoCard;
        _isPrimaryDisplay = isPrimaryDisplay;
        Width = width;
        Height = height;
        Address = address;
        _index = index;
        Scale = scale;
        MainWindow.AppClosing += MainWindow_AppClosing;
        _frameRenderTimeWatch = new Stopwatch();
        _bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
    }

    private void DrawThreadMethod() {
        while (!_exitDrawThread) {
            _drawAction?.Invoke();
            if (!_exitDrawThread) {
                _manualResetEvent.WaitOne();
            }
        }
    }

    private Action? UIUpdateMethod { get; set; }

    internal void SetUIUpdateMethod(Action invalidateImageAction) {
        UIUpdateMethod = invalidateImageAction;
    }

    [RelayCommand]
    public async Task SaveBitmap() {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null &&
            desktop.MainWindow.StorageProvider.CanSave &&
            desktop.MainWindow.StorageProvider.CanPickFolder) {
            IStorageProvider storageProvider = desktop.MainWindow.StorageProvider;
            FilePickerSaveOptions options = new() {
                Title = "Save bitmap image...",
                DefaultExtension = "bmp",
                SuggestedStartLocation = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
            };
            string? file = (await storageProvider.SaveFilePickerAsync(options))?.TryGetLocalPath();
            if (!string.IsNullOrWhiteSpace(file)) {
                Bitmap?.Save(file);
            }
        }
    }

    private void MainWindow_AppClosing(object? sender, System.ComponentModel.CancelEventArgs e) {
        _appClosing = true;
    }

    public uint Address { get; private set; }

    /// <summary>
    /// TODO : Get current DPI from Avalonia or Skia.
    /// It isn't DesktopScaling or RenderScaling as this returns 1 when Windows Desktop Scaling is set at 100%
    /// DPI: AvaloniaUI, like WPF, renders UI Controls in Device Independant Pixels.<br/>
    /// According to searches online, DPI is tied to a TopLevel control (a Window).<br/>
    /// Right now, the DPI is hardcoded for WriteableBitmap : https://github.com/AvaloniaUI/Avalonia/issues/1292 <br/>
    /// See also : https://github.com/AvaloniaUI/Avalonia/pull/1889 <br/>
    /// Also WriteableBitmap is an IImage implementation and not a UI Control,<br/>
    /// that's why it's used to bind the Source property of the Image control in VideoBufferView.xaml<br/>
    /// </summary>
    [ObservableProperty]
    private WriteableBitmap? _bitmap;

    private bool _showCursor = false;

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

    [ObservableProperty]
    private Cursor? _cursor = Cursor.Default;

    private double _scale = 1;

    public double Scale {
        get => _scale;
        set => SetProperty(ref _scale, Math.Max(value, 1));
    }

    [ObservableProperty]
    private int _height = 200;

    [ObservableProperty]
    private bool _isPrimaryDisplay;

    [ObservableProperty]
    private int _width = 320;

    [ObservableProperty]
    private long _framesRendered;

    private bool _appClosing;

    private readonly int _index;

    public int CompareTo(VideoBufferViewModel? other) {
        if (_index < other?._index) {
            return -1;
        }
        if (_index == other?._index) {
            return 0;
        }
        return 1;
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private readonly Stopwatch _frameRenderTimeWatch;

    private Action? _drawAction;

    public void Draw() {
        if (_appClosing || _disposedValue || UIUpdateMethod is null || Bitmap is null) {
            return;
        }
        if (_drawThread is null) {
            _drawThread = new Thread(DrawThreadMethod) {
                Name = "UIRenderThread"
            };
            _drawThread.Start();
        }
        _drawAction ??= () => {
            _frameRenderTimeWatch.Restart();
            using ILockedFramebuffer pixels = Bitmap.Lock();
            _videoCard?.Render(Address, Width, Height, pixels.Address);

            Dispatcher.UIThread.Post(() => {
                UIUpdateMethod?.Invoke();
                FramesRendered++;
            }, DispatcherPriority.Render);
            _frameRenderTimeWatch.Stop();
            LastFrameRenderTimeMs = _frameRenderTimeWatch.ElapsedMilliseconds;
        };
        if (!_exitDrawThread) {
            _manualResetEvent.Set();
            _manualResetEvent.Reset();
        }
    }

    [ObservableProperty]
    private long _lastFrameRenderTimeMs;

    private readonly IVideoCard? _videoCard;

    public override bool Equals(object? obj) {
        return this == obj || ((obj is VideoBufferViewModel other) && _index == other._index);
    }

    public override int GetHashCode() {
        return _index;
    }

    private void Dispose(bool disposing) {
        if (!_disposedValue) {
            if (disposing) {
                _exitDrawThread = true;
                _manualResetEvent.Set();
                if (_drawThread?.IsAlive == true) {
                    _drawThread.Join();
                }
                _manualResetEvent.Dispose();
                Dispatcher.UIThread.Post(() => {
                    Bitmap?.Dispose();
                    Bitmap = null;
                    Cursor?.Dispose();
                    UIUpdateMethod?.Invoke();
                }, DispatcherPriority.MaxValue);
            }
            _disposedValue = true;
        }
    }
}