using CommunityToolkit.WinUI.UI.Controls;
using SAaP.ViewModels;
using System.Linq.Dynamic.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace SAaP.Views;

/// <summary>
/// Analyze Detail Page
/// </summary>
public sealed partial class AnalyzeDetailPage
{
	public AnalyzeDetailViewModel ViewModel { get; }

	public AnalyzeDetailPage()
	{
		ViewModel = App.GetService<AnalyzeDetailViewModel>();
		this.InitializeComponent();
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		var codeName = e.Parameter as string;

		if (string.IsNullOrEmpty(codeName)) return;

		ViewModel.CodeName = codeName;

		await ViewModel.Initialize();
		ViewModel.OnQueryFinishEvent += (_, _) =>
		{
			AnalyzeReport.ReportSummary = ViewModel.AnalyzeWeeklySummary;
			CompareReport.ReportSummary = ViewModel.CompareWeeklySummary;
		};
		await ViewModel.AnalyzeStartCommand.ExecuteAsync(MainCan);

		PickerEnd.Date = DateTime.Now;
	}

	private void DataGrid_OnSorting(object sender, DataGridColumnEventArgs e)
	{
		// just for sure
		if (e.Column.Tag == null) return;

		// args for linq dynamic sorting
		string args;

		foreach (var column in AnalyzeResultGrid.Columns)
		{
			// self skip
			if (e.Column == column) continue;
			// clear others sort direction
			column.SortDirection = null;
		}

		// direction decide
		if (e.Column.SortDirection is null or DataGridSortDirection.Descending)
		{
			e.Column.SortDirection = DataGridSortDirection.Ascending;
			args = $"{e.Column.Tag} desc";
		}
		else
		{
			e.Column.SortDirection = DataGridSortDirection.Descending;
			args = $"{e.Column.Tag} asc";
		}

		// sort using linq dynamic && update item source
		AnalyzeResultGrid.ItemsSource = ViewModel.AnalyzedResults.AsQueryable().OrderBy(args);
	}

	private void CompareRelation_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{

	}

	private void CustomRange_OnToggled(object sender, RoutedEventArgs e)
	{
		if (CustomRange.IsOn)
		{
			PickerStart.IsEnabled = true;
			PickerEnd.IsEnabled = true;
		}
		else
		{
			PickerStart.IsEnabled = false;
			PickerEnd.IsEnabled = false;
		}
	}
}