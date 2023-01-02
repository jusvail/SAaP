using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Mapster;
using SAaP.Contracts.Services;
using SAaP.Core.Models;
using SAaP.Core.Services;
using SAaP.Chart.Contracts.Services;
using SAaP.Core.Services.Analyze;
using SAaP.Models;
using SAaP.Core.Models.DB;

namespace SAaP.ViewModels;

public class AnalyzeDetailViewModel : ObservableRecipient
{
    public ObservableCollection<AnalysisResultDetail> AnalyzedResults { get; } = new();

    public readonly IList<int> DefaultDuration = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 40, 50, 60, 80, 100, 120, 150, 200, 250 };

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
    private string _relationPercent;
    private string _selectedCompareRelationValue;
    private bool _isCustomRangeOn;
    private DateTimeOffset _customDateStart = DateTimeOffset.Now.AddDays(-28);
    private DateTimeOffset _customDateEnd = DateTimeOffset.Now;

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

    public string SelectedCompareRelationValue
    {
        get => _selectedCompareRelationValue;
        set => SetProperty(ref _selectedCompareRelationValue, value);
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

    public string RelationPercent
    {
        get => _relationPercent;
        set => SetProperty(ref _relationPercent, value);
    }

    public bool IsCustomRangeOn
    {
        get => _isCustomRangeOn;
        set => SetProperty(ref _isCustomRangeOn, value);
    }

    public DateTimeOffset CustomDateStart
    {
        get => _customDateStart;
        set => SetProperty(ref _customDateStart, value);
    }

    public DateTimeOffset CustomDateEnd
    {
        get => _customDateEnd;
        set => SetProperty(ref _customDateEnd, value);
    }

    public ObservableCollection<InDayDetail> AnalyzeDayDetails { get; set; } = new();

    public ObservableCollection<InDayDetail> CompareDayDetails { get; set; } = new();

    public WeeklyReportSummary AnalyzeWeeklySummary { get; set; } = new();

    public WeeklyReportSummary CompareWeeklySummary { get; set; } = new();

    public event EventHandler OnQueryFinishEvent;

    public IAsyncRelayCommand<object> AnalyzeStartCommand { get; }

    public AnalyzeDetailViewModel(IStockAnalyzeService stockAnalyzeService, IChartService chartService, IDbTransferService dbTransferService, IFetchStockDataService fetchStockDataService, IWindowManageService windowManageService)
    {
        _stockAnalyzeService = stockAnalyzeService;
        _chartService = chartService;
        _dbTransferService = dbTransferService;
        _fetchStockDataService = fetchStockDataService;
        _windowManageService = windowManageService;

        AnalyzeStartCommand = new AsyncRelayCommand<object>(StartAnalyze);
    }

    public async Task StartAnalyze(object canvas)
    {
        if (canvas is not Canvas realCanvas) return;

        var compareCode = ComparerCheck switch
        {
            "1" => StockService.ShZs,
            "0" => StockService.SzCz,
            "2" => OtherComparer,
            _ => throw new ArgumentOutOfRangeException(nameof(ComparerCheck))
        };

        // fetch stock data from tdx, then store to csv file
        await _fetchStockDataService.FetchStockData(compareCode);

        int duration;

        if (SelectedCompareRelationIndex > 0 && SelectedCompareRelationIndex < DefaultDuration.Count - 1)
        {
            duration = DefaultDuration[SelectedCompareRelationIndex];
        }
        else
        {
            try
            {
                duration = Convert.ToInt32(SelectedCompareRelationValue);
            }
            catch (Exception)
            {
                duration = 20;
            }
        }

        var codeList = new[] { CodeName, compareCode };

        var originalDatasList = new List<List<OriginalData>>();

        foreach (var codeName in codeList)
        {
            if (string.IsNullOrEmpty(codeName)) continue;

            var belong = await _fetchStockDataService.TryGetBelongByCode(codeName);

            List<OriginalData> originalData;

            if (IsCustomRangeOn)
            {
                // query original data range
                originalData = await DbService.TakeOriginalData(StockService.CutStockCodeToSix(codeName), belong, CustomDateStart.LocalDateTime, CustomDateEnd.LocalDateTime);

                if (originalData.Any()) SelectedCompareRelationValue = originalData.Count.ToString();
            }
            else
            {
                // query original data recently
                originalData = await DbService.TakeOriginalData(StockService.CutStockCodeToSix(codeName), belong, duration);
            }

            if (!originalData.Any()) return;

            originalDatasList.Add(originalData);
        }

        await StartDraw(realCanvas, codeList, originalDatasList);

        ListHistory(originalDatasList);

        AnalyzeWeeklySummary.CompanyName = await DbService.SelectCompanyNameByCode(CodeName);
        CompareWeeklySummary.CompanyName = await DbService.SelectCompanyNameByCode(compareCode);

        OnQueryFinishEvent?.Invoke(null, EventArgs.Empty);

        // bring window back
        var xamlRoot = _windowManageService.GetWindowForElement(realCanvas.XamlRoot, typeof(AnalyzeDetailViewModel).FullName!);
        _windowManageService.SetWindowForeground(xamlRoot);
    }

    private void ListHistory(IReadOnlyList<List<OriginalData>> originalDatasList)
    {
        SetHistory(AnalyzeDayDetails, originalDatasList[0]);
        SetHistory(CompareDayDetails, originalDatasList[1]);

        AnalyzeWeeklySummary.Clear();
        CompareWeeklySummary.Clear();

        GenerateWeeklyReport(AnalyzeDayDetails, AnalyzeWeeklySummary);
        GenerateWeeklyReport(CompareDayDetails, CompareWeeklySummary);

    }

    private static void GenerateWeeklyReport(IReadOnlyCollection<InDayDetail> dayDetails, WeeklyReportSummary summary)
    {
        GenerateWeeklyReport(dayDetails.Where(d => d.IsTradingDay && d.DayTime.DayOfWeek == DayOfWeek.Monday).ToList(), summary.Monday);
        GenerateWeeklyReport(dayDetails.Where(d => d.IsTradingDay && d.DayTime.DayOfWeek == DayOfWeek.Tuesday).ToList(), summary.Tuesday);
        GenerateWeeklyReport(dayDetails.Where(d => d.IsTradingDay && d.DayTime.DayOfWeek == DayOfWeek.Wednesday).ToList(), summary.Wednesday);
        GenerateWeeklyReport(dayDetails.Where(d => d.IsTradingDay && d.DayTime.DayOfWeek == DayOfWeek.Thursday).ToList(), summary.Thursday);
        GenerateWeeklyReport(dayDetails.Where(d => d.IsTradingDay && d.DayTime.DayOfWeek == DayOfWeek.Friday).ToList(), summary.Friday);
    }

    private static void GenerateWeeklyReport(ICollection<InDayDetail> dayDetails, WeeklyReport summary)
    {
        if (!dayDetails.Any()) return;

        var detailsCount = dayDetails.Count;

        var g0d = dayDetails.Count(inDayDetail => inDayDetail.Zd > 0);
        var l0d = dayDetails.Count - g0d;

        summary.ZdDistribute = $"{g0d}/{l0d}";

        // ReSharper disable once PossibleLossOfFraction
        summary.ZdPercent = CalculationService.Round2(100 * g0d / detailsCount) + "%";

        var sumG0 = dayDetails.Where(inDayDetail => inDayDetail.Zd > 0).Sum(inDayDetail => inDayDetail.Zd);

        summary.AverageZf = CalculationService.Round2(sumG0 / g0d);

        var sumL0 = dayDetails.Where(inDayDetail => inDayDetail.Zd < 0).Sum(inDayDetail => inDayDetail.Zd);

        summary.AverageDf = CalculationService.Round2(sumL0 / l0d);

        summary.ZtCount = dayDetails.Count(inDayDetail => inDayDetail.Zd > 9.8);
        summary.DtCount = dayDetails.Count(inDayDetail => inDayDetail.Zd < -9.8);

        var opg0d = dayDetails.Count(inDayDetail => inDayDetail.Op > 0);
        var opg1d = dayDetails.Count(inDayDetail => inDayDetail.Op > 1);
        var opl0d = dayDetails.Count - opg0d;

        summary.OpDistribute = $"{opg0d}/{opl0d}";
        // ReSharper disable once PossibleLossOfFraction
        summary.OpPercent = CalculationService.Round2(100 * opg0d / detailsCount) + "%";
        // ReSharper disable once PossibleLossOfFraction
        summary.Op1PPercent = CalculationService.Round2(100 * opg1d / detailsCount) + "%";
        summary.AverageOp = CalculationService.Round2(dayDetails.Sum(inDayDetail => inDayDetail.Op) / detailsCount);
    }

    private static void SetHistory(IList<InDayDetail> details, IReadOnlyList<OriginalData> originalDatas)
    {
        details.Clear();

        var tmpDetail = new List<InDayDetail>();

        for (var i = originalDatas.Count - 2; i >= 0; i--)
        {
            var originalData = originalDatas[i];
            var day = originalData.Adapt<InDayDetail>();

            day.YesterdaysEnding = originalDatas[i + 1].Ending;

            day.Zd = CalculationService.CalcTtm(day.YesterdaysEnding, day.Ending);
            day.Op = CalculationService.CalcTtm(day.YesterdaysEnding, day.High);
            day.Zf = CalculationService.CalcTtm(day.Low, day.High);

            day.DayOfWeek = day.DayTime.DayOfWeek;

            tmpDetail.Add(day);
        }

        if (!tmpDetail.Any()) return;

        // 填充开头的工作日
        while (tmpDetail[0].DayTime.DayOfWeek != DayOfWeek.Monday)
        {
            var blankDay = new InDayDetail();

            var newDate = tmpDetail[0].DayTime.AddDays(-1);

            blankDay.DayTime = newDate;
            blankDay.DayOfWeek = newDate.DayOfWeek;

            tmpDetail.Insert(0, blankDay);
        }

        details.Add(tmpDetail[0]);

        var j = 1;
        while (j < tmpDetail.Count)
        {
            var curDw = (int)tmpDetail[j].DayTime.DayOfWeek;
            var prevDw = (int)details[^1].DayTime.DayOfWeek;

            var range = CheckNeededRange(prevDw, curDw);

            for (var k = 0; k < range; k++)
            {
                var blankDay = new InDayDetail();

                var newDate = details[^1].DayTime.AddDays(1);

                while (newDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    newDate = newDate.AddDays(1);
                }

                blankDay.DayTime = newDate;
                blankDay.DayOfWeek = newDate.DayOfWeek;

                details.Add(blankDay);
            }

            details.Add(tmpDetail[j]);

            j++;
        }
    }

    private static int MoveNext(int target)
    {
        return target switch
        {
            < 1 or > 5 => 1,
            5 => 1,
            _ => target + 1
        };
    }

    private static int CheckNeededRange(int start, int end)
    {
        var nxt = MoveNext(start);

        if (nxt == end) return 0;

        var range = 0;

        range++;

        while (true)
        {
            nxt = MoveNext(nxt);

            if (nxt == end)
            {
                break;
            }

            range++;
        }

        return range;
    }

    private async Task StartDraw(Canvas realCanvas, IReadOnlyList<string> codeList, IReadOnlyList<List<OriginalData>> originalDatasList)
    {
        // transfer stock data to sqlite database
        await Task.Run(async () =>
        {
            await _dbTransferService.TransferCsvDataToDb(codeList);
        });

        var col = new List<IList<double>>();
        var names = new List<string>();
        var days = new List<List<string>>();

        for (var i = 0; i < originalDatasList.Count; i++)
        {
            var originalData = originalDatasList[i];
            names.Add(await DbService.SelectCompanyNameByCode(codeList[i]));
            days.Add(originalData.Select(o => o.Day).Reverse().ToList());

            var bot = new AnalyzeBot(originalData);
            var data = ComparerModeCheck switch
            {
                "0" => bot.Ttm,
                "1" => bot.OverpricedList,
                _ => throw new ArgumentOutOfRangeException(nameof(ComparerModeCheck))
            };

            if (data != null) col.Add(data);
        }

        _chartService.DrawBar(realCanvas, col, names, days);

        RelationPercent = _stockAnalyzeService.CalcRelationPercent(col);

    }

    public async void Initialize()
    {
        if (string.IsNullOrEmpty(CodeName)) return;

        AnalyzedResults.Clear();

        foreach (var duration in DefaultDuration)
        {
            var data = await _stockAnalyzeService.Analyze(CodeName, duration);
            if (data != null)
            {
                AnalyzedResults.Add(data);
            }
        }

        ComparerCheck = "1";
        ComparerModeCheck = "0";

        _selectedCompareRelationIndex = 13; // 最近40天
    }
}