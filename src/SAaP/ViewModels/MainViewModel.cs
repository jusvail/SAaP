using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using System.Collections.ObjectModel;
using SAaP.Contracts.Services;
using SAaP.Core.Services;
using SAaP.Core.DataModels;

namespace SAaP.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private string _codeInput;
    private string _lastingDays;

    // store csv output by py script to sqlite database
    private readonly IDbTransferService _dbTransferService;
    // analyze main service
    private readonly IStockAnalyzeService _stockAnalyzeService;

    public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

    public IAsyncRelayCommand AnalysisPressedCommand { get; }

    public IAsyncRelayCommand AddToFavoriteCommand { get; }

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

    public IRelayCommand ClearDataGrid { get; }

    public MainViewModel()
    { }

    public MainViewModel(IDbTransferService dbTransferService, IStockAnalyzeService stockAnalyzeService)
    {
        _dbTransferService = dbTransferService;
        _stockAnalyzeService = stockAnalyzeService;
        AnalysisPressedCommand = new AsyncRelayCommand(OnAnalysisPressed);
        ClearDataGrid = new RelayCommand(OnClearDataGrid);
        AddToFavoriteCommand = new AsyncRelayCommand(AddToFavorite);
    }

    private async Task AddToFavorite()
    {
        var accuracyCodes = StringHelper.FormatInputCode(CodeInput);
        // TODO add to favorite func
    }

    private void OnClearDataGrid()
    {
        AnalyzedResults.Clear();
    }

    private async Task OnAnalysisPressed()
    {
        // check code accuracy
        var accuracyCodes = StringHelper.FormatInputCode(CodeInput);
        // check null input
        if (accuracyCodes == null) return;

        // add comma
        var pyArg = StockService.FormatPyArgument(accuracyCodes);

        // formatted code resetting
        CodeInput = pyArg;

        // python script execution
        await PythonService.RunPythonScript(PythonService.TdxReader, "C:/devEnv/Tools/TDX", StartupService.PyDataPath, pyArg);

        // TODO remove this after release
        // await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath));

        // write to sqlite database
        await _dbTransferService.TransferCsvDataToDb(accuracyCodes);

        //store last queried  codes
        LastQueriedCodes = accuracyCodes;

        // store this activity
        await _dbTransferService.StoreActivityDataToDb(DateTime.Now, pyArg, string.Empty);

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

        if (LastQueriedCodes == null) return;

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
    private void OnStockAnalyzeFinishedCallBack(AnalysisResult data)
    {
        AnalyzedResults.Add(data);
    }

    public async Task RestoreFavoriteCodesString()
    {
        // initial db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);



    }

    public async Task RestoreLastQueryString()
    {
        // get service
        var restoreSettingsService = App.GetService<IRestoreSettingsService>();
        // query from db
        var lastQuery = await restoreSettingsService.RestoreLastQueryStringFromDb();

        if (lastQuery != null)
        {
            CodeInput = lastQuery;
        }
    }
}