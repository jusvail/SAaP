using CommunityToolkit.Mvvm.ComponentModel;
using SAaP.Contracts.Services;
using SAaP.Core.Models;
using System.Collections.ObjectModel;
using Mapster;

namespace SAaP.ViewModels;

public class AnalyzeDetailViewModel : ObservableRecipient
{
	// analyze main service
	private readonly IStockAnalyzeService _stockAnalyzeService;

	private string _codeName;

	public string CodeName
	{
		get => _codeName;
		set => SetProperty(ref _codeName, value);
	}

	private readonly IList<int> _defaultDuration = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 20, 30, 40, 50, 80, 100, 120, 150, 200 };

	public AnalyzeDetailViewModel(IStockAnalyzeService stockAnalyzeService)
	{
		_stockAnalyzeService = stockAnalyzeService;
	}

	public ObservableCollection<AnalysisResultDetail> AnalyzedResults { get; } = new();

	public async void Initialize()
	{
		if (string.IsNullOrEmpty(CodeName)) return;

		AnalyzedResults.Clear();

		foreach (var duration in _defaultDuration)
		{
			await _stockAnalyzeService.Analyze(CodeName, duration,
				(analysisResult) =>
				{
					var detail = analysisResult.Adapt<AnalysisResultDetail>();
					detail.Duration = duration;
					AnalyzedResults.Add(detail);
				});
		}
	}

}
