using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using SAaP.Contracts.Services;
using SAaP.Core.Services.Generic;
using SAaP.Helper;
using SAaP.Services;
using WinRT.Interop;

namespace SAaP.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
	private readonly ILocalSettingsService _localSettingsService;
	private readonly IWindowManageService  _windowManageService;

	private string _pythonInstallationPath;
	private string _tdxInstallationPath;
	private string _versionDescription;

	public SettingsViewModel(ILocalSettingsService localSettingsService, IWindowManageService windowManageService)
	{
		_localSettingsService = localSettingsService;
		_windowManageService  = windowManageService;

		_versionDescription = RuntimeHelper.GetVersionDescription();

		OnPythonInstallationPathPressed = new AsyncRelayCommand<object>(OnPythonInstallationPath);
		OnTdxInstallationPathPressed    = new AsyncRelayCommand<object>(OnTdxInstallationPath);

		// delete the local cache folder  , and create again
		DeleteLocalStoredCacheCommand = new AsyncRelayCommand(StartupService.InitializeWorkSpaceFolderTreeAsync);
		// launch the local sqlite db location
		OpenLocalDbLocationCommand = new AsyncRelayCommand(async () =>
		{
			await Launcher.LaunchFolderAsync(
				await StorageFolder.GetFolderFromPathAsync(StartupService.DbPath));
		});
	}

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
	public IAsyncRelayCommand<object> OnTdxInstallationPathPressed  { get; }
	public IAsyncRelayCommand         DeleteLocalStoredCacheCommand { get; }
	public IAsyncRelayCommand         OpenLocalDbLocationCommand    { get; }


	public async Task ReadSettingsWhenInitialize()
	{
		PythonInstallationPath = await _localSettingsService.ReadSettingAsync<string>(nameof(PythonInstallationPath));
		TdxInstallationPath    = await _localSettingsService.ReadSettingAsync<string>(nameof(TdxInstallationPath));
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
		var window = WindowManageService.GetWindowForElement(element, typeof(NavigationRootPage).FullName!);

		var folderPicker = new FolderPicker();

		var hwnd = WindowNative.GetWindowHandle(window);
		InitializeWithWindow.Initialize(folderPicker, hwnd);

		folderPicker.FileTypeFilter.Add("*");
		var folder = await folderPicker.PickSingleFolderAsync();

		return folder == null ? string.Empty : folder.Path;
	}
}