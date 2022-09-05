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
    private string _lastingDays;

    // store csv output by py script to sqlite database
    private readonly ICsvToDbTransferService _csvToDbTransferService;
    // analyze main service
    private readonly IStockAnalyzeService _stockAnalyzeService;

    public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

    public IAsyncRelayCommand AnalysisPressedCommand { get; }

    private IList<string> LastQueriedCodes { get; set; }

    public string CodeInput
    {
        get => _codeInput;
        set => SetProperty(ref _codeInput, value);
    }

    public string LastingDays
    {
        get => _lastingDays;
        set => SetProperty(ref _lastingDays, value);
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

        //store last queried  codes
        LastQueriedCodes = accuracyCodes;

        // invoke analyze
        await OnLastingDaysValueChanged();
    }

    public async Task OnLastingDaysValueChanged()
    {
        // if none int
        if (!int.TryParse(LastingDays, out var duration)) return;

        // ignore less than 5 days analyze
        if (duration < 5) return;

        // clear preview result
        AnalyzedResults.Clear();

        // analyze start
        foreach (var code in LastQueriedCodes)
        {
            // analyze data
            await _stockAnalyzeService.Analyze(code, duration, OnStockAnalyzeFinishedCallBack);
        }
    }

    /// <summary>
    /// when analyze finished, pass result to ui
    /// </summary>
    /// <param name="data"></param>
    private Task OnStockAnalyzeFinishedCallBack(AnalysisResult data)
    {
        AnalyzedResults.Add(data);
        return Task.CompletedTask;
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