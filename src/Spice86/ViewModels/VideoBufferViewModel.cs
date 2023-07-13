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
using Spice86.Shared.Interfaces;

using System;
using System.Threading.Tasks;

/// <inheritdoc cref="Spice86.Shared.Interfaces.IVideoBufferViewModel" />
public sealed partial class VideoBufferViewModel : ObservableObject, IVideoBufferViewModel {
    private bool _disposedValue;

    private Thread? _drawThread;

    private bool _exitDrawThread;

    /// <summary>
    /// For AvaloniaUI Designer
    /// </summary>
    public VideoBufferViewModel() {
        if (Design.IsDesignMode == false) {
            throw new InvalidOperationException("This constructor is not for runtime usage");
        }
        Width = 320;
        Height = 200;
        Scale = 1;
    }

    public VideoBufferViewModel(IVideoCard videoCard, double scale, int width, int height) {
        _videoCard = videoCard;
        Width = width;
        Height = height;
        Scale = scale;
        MainWindow.AppClosing += MainWindow_AppClosing;
        _bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
    }

    private void DrawThreadMethod() {
        while (!_exitDrawThread) {
            _drawAction?.Invoke();
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
    private int _width = 320;

    private bool _appClosing;

    private Action? _drawAction;

    public void Draw() {
        if (_appClosing || _disposedValue || _videoCard is null || UIUpdateMethod is null || Bitmap is null) {
            return;
        }
        if (_drawThread is null) {
            _drawThread = new Thread(DrawThreadMethod) {
                Name = "UIRenderThread"
            };
            _drawThread.Start();
        }
        
        _drawAction ??= () => {
            unsafe {
                using ILockedFramebuffer pixels = Bitmap.Lock();
                var buffer = new Span<uint>((void*)pixels.Address, pixels.RowBytes * pixels.Size.Height / 4);
                _videoCard?.Render(buffer);

                Dispatcher.UIThread.Post(() => {
                    UIUpdateMethod?.Invoke();
                }, DispatcherPriority.Render);
            }
        };
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private readonly IVideoCard? _videoCard;

    private void Dispose(bool disposing) {
        if (!_disposedValue) {
            if (disposing) {
                _exitDrawThread = true;
                if (_drawThread?.IsAlive == true) {
                    _drawThread.Join();
                }
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