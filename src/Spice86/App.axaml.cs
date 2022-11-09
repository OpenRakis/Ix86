namespace Spice86;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Microsoft.Win32;
using Spice86.ViewModels;

using System;
using System.Runtime.Versioning;
using Spice86.Views;

internal partial class App : Application {
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private const string RegistryValueName = "AppsUseLightTheme";

    public static MainWindow? MainWindow { get; private set; }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (!IsInDarkMode()) {
            Styles.RemoveAt(1);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            ExpressionObserver.DataValidators.RemoveAll(x => x is DataAnnotationsValidationPlugin);

            MainWindowViewModel mainViewModel = new MainWindowViewModel();
            mainViewModel.SetConfiguration(desktop.Args ?? Array.Empty<string>());
            desktop.MainWindow = new MainWindow {
                DataContext = mainViewModel,
            };
            desktop.MainWindow.Closed += (s, e) => mainViewModel.Dispose();
            desktop.MainWindow.Opened += mainViewModel.OnMainWindowOpened;
            MainWindow = (MainWindow)desktop.MainWindow;
        }
        base.OnFrameworkInitializationCompleted();
    }

    [SupportedOSPlatform("windows")]
    private static bool GetIsWindowsInDarkMode() {
        RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        object? registryValueObject = key?.GetValue(RegistryValueName);
        if (registryValueObject == null) {
            return false;
        }

        int registryValue = (int)registryValueObject;
        return registryValue <= 0;
    }

    private static bool IsInDarkMode() {
#pragma warning disable ERP022 // Unobserved exception in generic exception handler
        try {
            if (OperatingSystem.IsWindows()) {
                return GetIsWindowsInDarkMode();
            }
        } catch {
            //No OS support for themes. Not worth crashing for. Not worth reporting.
        }
#pragma warning restore ERP022 // Unobserved exception in generic exception handler
        return true;
    }
}