using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Contracts.Services;
using SAaP.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Mapster;

namespace SAaP.ViewModels;

public class AnalyzeDetailViewModel : ObservableRecipient
{
    public ObservableCollection<AnalysisResultDetail> AnalyzedResults { get; } = new();

    public readonly IList<int> DefaultDuration = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 40, 50, 80, 100, 120, 150, 200 };

    // analyze main service
    private readonly IStockAnalyzeService _stockAnalyzeService;

    private string _codeName;
    private string _comparerCheck;
    private int _selectedCompareRelationIndex;
    private string _comparerModeCheck;

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

    public IRelayCommand<object> DrawStartCommand { get; }

    public AnalyzeDetailViewModel(IStockAnalyzeService stockAnalyzeService)
    {
        _stockAnalyzeService = stockAnalyzeService;

        DrawStartCommand = new RelayCommand<object>(StartingDraw);
    }

    public void StartingDraw(object canvas)
    {

    }

    public async void Initialize()
    {
        if (string.IsNullOrEmpty(CodeName)) return;

        AnalyzedResults.Clear();

        foreach (var duration in DefaultDuration)
        {
            await _stockAnalyzeService.Analyze(CodeName, duration,
                analysisResult =>
                {
                    var detail = analysisResult.Adapt<AnalysisResultDetail>();
                    detail.Duration = duration;
                    AnalyzedResults.Add(detail);
                });
        }

        ComparerCheck = "1";
        ComparerModeCheck = "0";
    }
}
