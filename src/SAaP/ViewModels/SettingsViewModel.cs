using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Contracts.Services;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using SAaP.Helper;
using System.Reflection;
using Windows.ApplicationModel;
using SAaP.Extensions;

namespace SAaP.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private string _tdxInstallationPath;
    private string _pythonInstallationPath;
    private string _versionDescription;

    private readonly ILocalSettingsService _localSettingsService;

    private readonly IWindowManageService _windowManageService;

    public string PythonInstallationPath
    {
        get => _pythonInstallationPath;
        set => SetProperty(ref _pythonInstallationPath, value);
    }

    public string TdxInstallationPath
    {
        get => _tdxInstallationPath;
        set => SetProperty(ref _tdxInstallationPath, value);
    }
    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public IAsyncRelayCommand<object> OnPythonInstallationPathPressed { get; }

    public IAsyncRelayCommand<object> OnTdxInstallationPathPressed { get; }

    public SettingsViewModel(ILocalSettingsService localSettingsService, IWindowManageService windowManageService)
    {
        _localSettingsService = localSettingsService;
        _windowManageService = windowManageService;

        _versionDescription = GetVersionDescription();

        OnPythonInstallationPathPressed = new AsyncRelayCommand<object>(OnPythonInstallationPath);
        OnTdxInstallationPathPressed = new AsyncRelayCommand<object>(OnTdxInstallationPath);
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    public async Task ReadSettingsWhenInitialize()
    {
        PythonInstallationPath = await _localSettingsService.ReadSettingAsync<string>(nameof(PythonInstallationPath));
        TdxInstallationPath = await _localSettingsService.ReadSettingAsync<string>(nameof(TdxInstallationPath));
    }

    private async Task OnPythonInstallationPath(object xamlRoot)
    {
        var folder = await PickFromFileDirectory(xamlRoot as XamlRoot);

        if (string.IsNullOrEmpty(folder)) return;

        PythonInstallationPath = folder;

        await _localSettingsService.SaveSettingAsync(nameof(PythonInstallationPath), PythonInstallationPath);
    }


    private async Task OnTdxInstallationPath(object xamlRoot)
    {
        var folder = await PickFromFileDirectory(xamlRoot as XamlRoot);

        if (string.IsNullOrEmpty(folder)) return;

        TdxInstallationPath = folder;

        await _localSettingsService.SaveSettingAsync(nameof(TdxInstallationPath), TdxInstallationPath);
    }

    private async Task<string> PickFromFileDirectory(XamlRoot element)
    {
        var window = _windowManageService.GetWindowForElement(element);

        var folderPicker = new FolderPicker();

        var hwnd = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();

        return folder == null ? string.Empty : folder.Path;
    }
}