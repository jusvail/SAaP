using System.Linq.Dynamic.Core;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Contracts.Services;
using SAaP.Core.Services.Generic;
using SAaP.Extensions;
using SAaP.Models;
using SAaP.ViewModels;

namespace SAaP.Views;

public sealed partial class SimulatePage
{
	public SimulatePage()
	{
		ViewModel = App.GetService<SimulatePageViewModel>();
		InitializeComponent();
	}

	public SimulatePageViewModel ViewModel { get; set; }

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		if (e.Parameter is not ObservableTaskDetail task) return;

		ViewModel.TaskDetail = task;
		// if (ViewModel.TaskDetail.FilteredStock.Any())
		// {
		// 	var sb = new StringBuilder();
		// 	foreach (var code in ViewModel.TaskDetail.FilteredStock.Select(s => s.CodeNameFull).ToList())
		// 	{
		// 		sb.Append(code).Append(",");
		// 	}
		// 	ViewModel.CodeInput = sb.ToString();
		// 	await ViewModel.FormatCodeInput(ViewModel.CodeInput);
		// }
		await ViewModel.RestoreLastQueryStringAsync();
		ViewModel.InitializeField();

		base.OnNavigatedTo(e);
	}


	private async void CodeNameCell_OnClick(object sender, RoutedEventArgs e)
	{
		var hyb = sender as HyperlinkButton;
		if (hyb == null) return;

		var codeName = hyb.Content as string;

		if (string.IsNullOrEmpty(codeName)) return;

		codeName = StockService.ReplaceLocStringToFlag(codeName);

		var companyName = await StockService.FetchCompanyNameByCode(codeName).ConfigureAwait(true);

		var title = "AnalyzeDetailPageTitle".GetLocalized() + $": [{codeName} {companyName}]";

		App.GetService<IWindowManageService>().CreateOrBackToWindow<AnalyzeDetailPage>(typeof(AnalyzeDetailViewModel).FullName!, title,
																					   codeName);
	}


	private async void OnCodeInputLostFocusEventHandler(object sender, RoutedEventArgs e)
	{
		await ViewModel.FormatCodeInput(CodeInput.Text);
	}

	private void DataGrid_OnSorting(object sender, DataGridColumnEventArgs e)
	{
		try
		{
			// just for sure
			if (e.Column.Tag == null) return;

			// args for linq dynamic sorting
			string args;

			foreach (var column in SimulateResultDataGrid.Columns)
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
			SimulateResultDataGrid.ItemsSource = ViewModel.SimulateResults.AsQueryable().OrderBy(args);

		}
		catch (Exception exception)
		{
			Console.WriteLine(exception);
		}
	}
}