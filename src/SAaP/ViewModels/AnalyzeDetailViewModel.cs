using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using SAaP.Contracts.Services;
using SAaP.Core.Models;
using SAaP.Core.Services;
using SAaP.Chart.Contracts.Services;

namespace SAaP.ViewModels;

public class AnalyzeDetailViewModel : ObservableRecipient
{
    public ObservableCollection<AnalysisResultDetail> AnalyzedResults { get; } = new();

    public readonly IList<int> DefaultDuration = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 40, 50, 80, 100, 120, 150, 200 };

    // analyze main service
    private readonly IStockAnalyzeService _stockAnalyzeService;
    private readonly IChartService _chartService;
    private readonly IDbTransferService _dbTransferService;
    private readonly IFetchStockDataService _fetchStockDataService;
    private readonly IWindowManageService _windowManageService;

    private int _selectedCompareRelationIndex;
    private string _codeName;
    private string _comparerCheck;
    private string _comparerModeCheck;
    private string _otherComparer;

    public string CodeName
    {
        get => _codeName;
        set => SetProperty(ref _codeName, value);
    }

    public int SelectedCompareRelationIndex
    {
        get => _selectedCompareRelationIndex;
        set => SetProperty(ref _selectedCompareRelationIndex, value);
    }

    public string ComparerCheck
    {
        get => _comparerCheck;
        set
        {
            if (value != null)
            {
                SetProperty(ref _comparerCheck, value);
            }
        }
    }

    public string ComparerModeCheck
    {
        get => _comparerModeCheck;
        set
        {
            if (value != null)
            {
                SetProperty(ref _comparerModeCheck, value);
            }
        }
    }

    public string OtherComparer
    {
        get => _otherComparer;
        set => SetProperty(ref _otherComparer, value);
    }

    public IRelayCommand<object> DrawStartCommand { get; }

    public AnalyzeDetailViewModel(IStockAnalyzeService stockAnalyzeService, IChartService chartService, IDbTransferService dbTransferService, IFetchStockDataService fetchStockDataService, IWindowManageService windowManageService)
    {
        _stockAnalyzeService = stockAnalyzeService;
        _chartService = chartService;
        _dbTransferService = dbTransferService;
        _fetchStockDataService = fetchStockDataService;
        _windowManageService = windowManageService;

        DrawStartCommand = new RelayCommand<object>(StartingDraw);
    }

    public async void StartingDraw(object canvas)
    {
        var realCanvas = canvas as Canvas;

        if (realCanvas == null) return;

        var duration = DefaultDuration[SelectedCompareRelationIndex];

        var compareCode = ComparerCheck switch
        {
            "1" => StockService.ShZs,
            "0" => StockService.SzCz,
            "2" => OtherComparer,
            _ => throw new ArgumentOutOfRangeException(nameof(ComparerCheck))
        };

        // get latest compareCode's data

        // fetch stock data from tdx, then store to csv file
        await _fetchStockDataService.FetchStockData(compareCode);

        // transfer stock data to sqlite database
        await Task.Run(async () =>
        {
            await _dbTransferService.TransferCsvDataToDb(new List<string> { compareCode });
        });

        var datas = await AnalyzeData(duration, CodeName, compareCode);

        _chartService.DrawBar(realCanvas, datas.ToList());

        // bring window back
        var xamlRoot = _windowManageService.GetWindowForElement(realCanvas.XamlRoot, typeof(AnalyzeDetailViewModel).FullName!);
        _windowManageService.SetWindowForeground(xamlRoot);
    }

    private async Task<IEnumerable<IList<double>>> AnalyzeData(int duration, params string[] codeNames)
    {
        var col = new List<IList<double>>();

        foreach (var codeName in codeNames)
        {
            if (string.IsNullOrEmpty(codeName)) continue;

            IList<double> data = null;

            await _stockAnalyzeService.GetTtm(codeName, duration, bot =>
            {
                switch (ComparerModeCheck)
                {
                    case "0":
                        data = bot.Ttm;
                        return;
                    case "1":
                        data = bot.OverpricedList;
                        return;
                    default: throw new ArgumentOutOfRangeException(nameof(ComparerModeCheck));
                }
            });

            if (data != null) col.Add(data);
        }

        return col;
    }

    public async void Initialize()
    {
        if (string.IsNullOrEmpty(CodeName)) return;

        AnalyzedResults.Clear();

        foreach (var duration in DefaultDuration)
        {
            await _stockAnalyzeService.Analyze(CodeName, duration, ana => AnalyzedResults.Add(ana));
        }

        ComparerCheck = "1";
        ComparerModeCheck = "0";

        _selectedCompareRelationIndex = 9;
    }
}