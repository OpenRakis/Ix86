namespace Spice86.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using Spice86.ViewModels;

using System;
using System.ComponentModel;

internal partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        Closing += MainWindow_Closing;
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private Image? _primaryDisplay = null;

    public void SetPrimaryDisplayControl(Image image) {
        if(_primaryDisplay != image) {
            _primaryDisplay = image;
            FocusOnPrimaryVideoBuffer();
            _primaryDisplay.KeyUp -= OnPrimaryDisplayOnKeyUp;
            _primaryDisplay.KeyDown -= OnPrimaryDisplayOnKeyDown;
            _primaryDisplay.KeyUp += OnPrimaryDisplayOnKeyUp;
            _primaryDisplay.KeyDown += OnPrimaryDisplayOnKeyDown;
        }
    }

    void OnPrimaryDisplayOnKeyUp(object? _, KeyEventArgs e) => (DataContext as MainWindowViewModel)?.OnKeyUp(e);
    void OnPrimaryDisplayOnKeyDown(object? _, KeyEventArgs e) => (DataContext as MainWindowViewModel)?.OnKeyDown(e);
    
    private void FocusOnPrimaryVideoBuffer() {
        if (_primaryDisplay is not null) {
            _primaryDisplay.IsEnabled = false;
            FocusManager.Instance?.Focus(_primaryDisplay);
            _primaryDisplay.IsEnabled = true;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e) {
        FocusOnPrimaryVideoBuffer();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        FocusOnPrimaryVideoBuffer();
    }

    public static event EventHandler<CancelEventArgs>? AppClosing;

    private void MainWindow_Closing(object? sender, CancelEventArgs e) {
        AppClosing?.Invoke(sender, e);
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}