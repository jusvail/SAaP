using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using SAaP.Contracts.Services;
using SAaP.Core.Services;

namespace SAaP.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private string _codeInput;

    private ICsvToDbTransferService _csvToDbTransferService;

    public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

    public ICommand AnalysisPressedCommand { get; }

    public string CodeInput
    {
        get => _codeInput;
        set => SetProperty(ref _codeInput, value);
    }

    public MainViewModel() { }

    public MainViewModel(ICsvToDbTransferService csvToDbTransferService)
    {
        _csvToDbTransferService = csvToDbTransferService;
        AnalysisPressedCommand = new RelayCommand(OnAnalysisPressed);
    }

    private async void OnAnalysisPressed()
    {
        // format input
        var codes = StringHelper.FormattingWithComma(CodeInput);

        // check code accuracy
        var accuracyCodes = StockService.CheckStockCodeAccuracy(codes).ToList();
        // add comma
        var pyArg = StockService.FormatPyArgument(accuracyCodes);

        // formatted code resetting
        CodeInput = pyArg;

        // python script execution
        await PythonService.RunPythonScript(PythonService.TdxReader, "C:/devEnv/Tools/TDX", StartupService.PyDataPath, pyArg);

        // TODO remove this after release
        await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath));

        // write to sqlite database
        _csvToDbTransferService.Transfer(accuracyCodes);


        // TODO analyze data

        // TODO show to UI data gram
    }
}