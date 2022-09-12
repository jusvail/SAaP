using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Contracts.Services;
using WinRT.Interop;
using Microsoft.UI.Xaml;

namespace SAaP.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private string _tdxInstallationPath;
    private string _pythonInstallationPath;

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

    public IAsyncRelayCommand<object> OnPythonInstallationPathPressed { get; }

    public IAsyncRelayCommand<object> OnTdxInstallationPathPressed { get; }

    private readonly ILocalSettingsService _localSettingsService;

    private readonly IWindowManageService _windowManageService;


    public SettingsViewModel(ILocalSettingsService localSettingsService, IWindowManageService windowManageService)
    {
        _localSettingsService = localSettingsService;
        _windowManageService = windowManageService;

        OnPythonInstallationPathPressed = new AsyncRelayCommand<object>(OnPythonInstallationPath);
        OnTdxInstallationPathPressed = new AsyncRelayCommand<object>(OnTdxInstallationPath);
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

        if (window == null) return null;

        var folderPicker = new FolderPicker();

        var hwnd = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();

        return folder == null ? string.Empty : folder.Path;
    }
}