using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.System;
using Mapster;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Services;
using SAaP.Core.Services;
using SAaP.Core.Models.DB;
using CommunityToolkit.WinUI.UI.Controls;

namespace SAaP.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private string _codeInput;

    // store csv output by py script to sqlite database
    private readonly ICsvToDbTransferService _csvToDbTransferService;
    // analyze main service
    private readonly IStockAnalyzeService _stockAnalyzeService;

    public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

    public IAsyncRelayCommand AnalysisPressedCommand { get; }

    public string CodeInput
    {
        get => _codeInput;
        set => SetProperty(ref _codeInput, value);
    }

    public MainViewModel()
    { }

    public MainViewModel(ICsvToDbTransferService csvToDbTransferService, IStockAnalyzeService stockAnalyzeService)
    {
        _csvToDbTransferService = csvToDbTransferService;
        _stockAnalyzeService = stockAnalyzeService;
        AnalysisPressedCommand = new AsyncRelayCommand(OnAnalysisPressed);
    }

    private async Task OnAnalysisPressed()
    {
        // check code accuracy
        var accuracyCodes = FormatInputCode(CodeInput);
        // check null input
        if (accuracyCodes == null) return;

        // add comma
        var pyArg = StockService.FormatPyArgument(accuracyCodes);

        // python script execution
        await PythonService.RunPythonScript(PythonService.TdxReader, "C:/devEnv/Tools/TDX", StartupService.PyDataPath, pyArg);

        // TODO remove this after release
        // await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath));

        // write to sqlite database
        await _csvToDbTransferService.Transfer(accuracyCodes);

        // analyze start
        foreach (var code in accuracyCodes)
        {
            // analyze data
            await _stockAnalyzeService.Analyze(code, 20, OnStockAnalyzeFinishedCallBack);
        }
    }

    /// <summary>
    /// when analyze finished, pass result to ui
    /// </summary>
    /// <param name="data"></param>
    private void OnStockAnalyzeFinishedCallBack(AnalysisResult data)
    {
        AnalyzedResults.Add(data);
    }

    private List<string> FormatInputCode(string codeInput)
    {
        // format input
        var codes = StringHelper.FormattingWithComma(codeInput);
        // check null input
        if (codes == null) return null;

        // check code accuracy
        var accuracyCodes = StockService.CheckStockCodeAccuracy(codes).ToList();
        // check null code
        if (accuracyCodes.Count == 0) return null;

        // add comma
        var pyArg = StockService.FormatPyArgument(accuracyCodes);

        // formatted code resetting
        CodeInput = pyArg;

        return accuracyCodes;
    }

    public void OnCodeInputLostFocusEventHandler(object sender, RoutedEventArgs e)
    {
        FormatInputCode((sender as TextBox)?.Text);
    }
}