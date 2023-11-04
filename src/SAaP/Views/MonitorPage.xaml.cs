using System.Collections.ObjectModel;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Services.Generic;
using SAaP.Helper;
using SAaP.Models;
using SAaP.ViewModels;

namespace SAaP.Views;

public sealed partial class MonitorPage
{
	public MonitorPage()
	{
		ViewModel = App.GetService<MonitorViewModel>();

		InitializeComponent();
	}

	public MonitorViewModel ViewModel { get; }

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		
		await ViewModel.InitializeSuggestData();
		await ViewModel.InitializeTrackData();
		await ViewModel.InitializeMonitorStockData();

		await ViewModel.RestoreSavedTasks();

		ViewModel.CurrentMonitorData.BuyModes[0].IsChecked = true;

		await ViewModel.LiveTask();
	}

	private void AddToMonitorAppBarButton_OnClick(object sender, RoutedEventArgs e)
	{
		UiInvokeHelper.TriggerButtonPressed(AddToMonitorAppBarButtonHidden);
	}

	private void CodeSelectSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
	{
		if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
		if (string.IsNullOrEmpty(sender.Text))
		{
			CodeSelectSuggestBox.ItemsSource = null;
			return;
		}

		var splitText = sender.Text.Split(" ");
		CodeSelectSuggestBox.ItemsSource = ViewModel.GetCodeSelectSuggest(splitText);
	}

	private async void CodeSelectSuggestBox_OnQuerySubmitted(AutoSuggestBox sender,
															 AutoSuggestBoxQuerySubmittedEventArgs args)
	{
		if (args.ChosenSuggestion is Stock stock)
			await ViewModel.AddToMonitor(stock);
		else if (!string.IsNullOrEmpty(args.QueryText)) await ViewModel.AddToMonitor(args.QueryText);
		sender.Text = string.Empty;
	}

	private async void CodeSelectSuggestBox_OnSuggestionChosen(AutoSuggestBox sender,
															   AutoSuggestBoxSuggestionChosenEventArgs args)
	{
		await ViewModel.AddToMonitor(args.SelectedItem);
		sender.Text = string.Empty;
	}

	private void DeleteMonitor_OnClick(object sender, RoutedEventArgs e)
	{
		ViewModel.DeleteMonitorItem(((FrameworkElement)e.OriginalSource).DataContext);
	}

	private async void DeleteHistoryDeduceData_OnClick(object sender, RoutedEventArgs e)
	{
		await ViewModel.DeleteHistoryDeduce(((FrameworkElement)e.OriginalSource).DataContext);
	}

	private void HelperAppBarButton_OnClick(object sender, RoutedEventArgs e)
	{
		FilterTextBoxTeachingTip.IsOpen = true;
	}

	private void FilterTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
	{
		ViewModel.CurrentTrackFilterCondition.IsValid = ViewModel.CurrentTrackFilterCondition.TrackIndex > 0;
	}

	private void SaveFilterConditionButton_OnClick(object sender, RoutedEventArgs e)
	{
		SaveCondition.Flyout.Hide();
	}

	private async void DeleteFilterCondition_OnClick(object sender, RoutedEventArgs e)
	{
		var dataContext = (e.OriginalSource as MenuFlyoutItem)?.DataContext;

		await ViewModel.DeleteFilterTrackData(dataContext);
	}

	private void ModifyFilterCondition_OnClick(object sender, RoutedEventArgs e)
	{
		var dataContext = (e.OriginalSource as MenuFlyoutItem)?.DataContext;

		ViewModel.EditFilterTrackData(dataContext);

		UiInvokeHelper.TriggerButtonPressed(SaveCondition);
	}

	private void CopyFilterCondition_OnClick(object sender, RoutedEventArgs e)
	{
		var dataContext = (e.OriginalSource as MenuFlyoutItem)?.DataContext;

		ViewModel.CopyFilterTrackData(dataContext);
	}

	private void ViewResult_OnClick(object sender, RoutedEventArgs e)
	{
		if (sender is not Button button) return;

		button.CommandParameter = FilterResultTabView;
	}

	private void FilterResultTabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
	{
		sender.TabItems.Remove(args.Tab);
	}

	private async void HistoryDeduce_OnClick(object sender, RoutedEventArgs e)
	{
		var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;

		if (dataContext is not Stock stock) return;

		if (!ViewModel.HistoryDeduceData.MonitorStocks.Contains(stock))
		{
			ViewModel.HistoryDeduceData.MonitorStocks.Add(stock);
			await ViewModel.ReinsertToDb(ActivityData.HistoryDeduce, ViewModel.HistoryDeduceData.MonitorStocks);
		}

		LiveMonitorTabView.SelectedIndex = 1;
	}

	private void BuyModeSelect_OnChecked(object sender, RoutedEventArgs e)
	{
		foreach (var buyMode in ViewModel.CurrentMonitorData.BuyModes) buyMode.IsChecked = true;
	}

	private void BuyModeSelect_OnUnchecked(object sender, RoutedEventArgs e)
	{
		foreach (var buyMode in ViewModel.CurrentMonitorData.BuyModes) buyMode.IsChecked = false;
	}

	private void ImportConfirm_OnClick(object sender, RoutedEventArgs e)
	{
		ReadyToImportButton.Flyout.Hide();
	}

	private void MonitorSelectAll_OnClick(object sender, RoutedEventArgs e)
	{
		if (MonitorStocksListView.SelectedItems.Count == MonitorStocksListView.Items.Count)
			MonitorStocksListView.SelectedValue = false;
		else
			MonitorStocksListView.SelectAll();
	}

	private async void CopyResult_OnClick(object sender, RoutedEventArgs e)
	{
		var dataPackage = new DataPackage
		{
			RequestedOperation = DataPackageOperation.Copy
		};

		var sb = new StringBuilder();

		var button = (Button)sender;

		try
		{
			foreach (MonitorNotification t in SimGridView.Items)
				sb.Append(t.FullTime.ToString("yyyy/MM/dd HH:mm:ss")).Append("\t")
				  .Append(t.CodeName).Append("\t")
				  .Append(t.CompanyName).Append("\t")
				  .Append(t.Direction).Append("\t")
				  .Append(t.Price).Append("\t")
				  .Append(BuyMode.ModeDetails[t.SubmittedByMode]).Append("\t")
				  .Append(t.Message).Append("\t")
				  .Append(Environment.NewLine);

			dataPackage.SetText(sb.ToString());
			Clipboard.SetContent(dataPackage);
		}
		catch (Exception)
		{
			button.Content = "复制出错！";
		}

		button.Content = "完成！";

		await Task.Delay(1000);

		button.Content = "复制结果";
	}

	private async void CopySumResult_OnClick(object sender, RoutedEventArgs e)
	{
		var dataPackage = new DataPackage
		{
			RequestedOperation = DataPackageOperation.Copy
		};

		var sb = new StringBuilder();

		var button = (Button)sender;

		try
		{
			var profits = new List<double>();

			foreach (var t1 in SimGridView.Items)
			{
				var t = (MonitorNotification)t1;
				if (t.Direction != DealDirection.Sell) continue;
				sb.Append(t.FullTime.ToString("yyyy/MM/dd HH:mm:ss")).Append("\t")
				  .Append(t.CodeName).Append("\t")
				  .Append(t.CompanyName).Append("\t")
				  .Append(BuyMode.ModeDetails[t.SubmittedByMode]).Append("\t")
				  .Append(t.Profit).Append("\t")
				  .Append("持股时间(min):").Append("\t")
				  .Append(t.HoldTime).Append("\t")
				  .Append(t.Message).Append("\t")
				  .Append(Environment.NewLine);

				profits.Add(t.Profit);
			}

			var av = profits.Average();
			var avP = profits.Where(p => p >= 0).Average();
			var avK = profits.Where(p => p < 0).Average();

			var mP = profits.Max();
			var mk = profits.Min();

			// ReSharper disable once PossibleLossOfFraction
			var success = CalculationService.Round2(100 * profits.Count(p => p >= 0) / profits.Count);

			sb.Append("总收益(%)：").Append("\t").Append(profits.Sum()).Append("\t").Append(Environment.NewLine);
			sb.Append("平均收益(%)：").Append("\t").Append(av).Append("\t").Append(Environment.NewLine);
			sb.Append("平均正收益(%)：").Append("\t").Append(avP).Append("\t").Append(Environment.NewLine);
			sb.Append("平均负收益(%)：").Append("\t").Append(avK).Append("\t").Append(Environment.NewLine);
			sb.Append("最大正收益(%)：").Append("\t").Append(mP).Append("\t").Append(Environment.NewLine);
			sb.Append("最大负收益(%)：").Append("\t").Append(mk).Append("\t").Append(Environment.NewLine);
			sb.Append("胜率(%)：").Append("\t").Append(success).Append("\t").Append(Environment.NewLine);

			dataPackage.SetText(sb.ToString());
			Clipboard.SetContent(dataPackage);
		}
		catch (Exception)
		{
			button.Content = "复制出错！";
		}

		button.Content = "完成！";

		await Task.Delay(1000);

		button.Content = "复制统计结果";
	}

	private void AddToSim_OnClick(object sender, RoutedEventArgs e)
	{
		var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;

		if (dataContext is not MonitorNotification notification) return;

		var stock = ViewModel.MonitorStocks.First(s => s.CodeName == notification.CodeName);

		if (!ViewModel.HistoryDeduceData.MonitorStocks.Contains(stock))
			ViewModel.HistoryDeduceData.MonitorStocks.Add(stock);

		LiveMonitorTabView.SelectedIndex = 1;
	}

	private void SaveFilterTask_OnClick(object sender, RoutedEventArgs e)
	{
		FilterResultTabView.SelectedIndex = 0;
		FilterTaskListView.ItemsSource = null;
		FilterTaskListView.ItemsSource = ViewModel.FilterTasks;
	}

	private async void ResetClicked_OnClick(object sender, RoutedEventArgs e)
	{
		ViewModel.CurrentTrackFilterCondition.Clear();

		if (ConditionGridView.ItemsSource is not ObservableCollection<ObservableTrackCondition> children) return;

		ConditionGridView.ItemsSource = null;
		ConditionGridView.ItemsSource = children;
		foreach (var child in children)
		{
			child.IsChecked = false;
			await Task.Delay(5);
		}
	}

	private void DeleteFilterTask_OnClick(object sender, RoutedEventArgs e)
	{
		var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
		FilterTaskListView.ItemsSource = null;
		ViewModel.DeleteFilterTask(dataContext);

		// var selected = FilterTaskListView.SelectedItems;
		// if (selected != null && selected.Any())
		// 	foreach (var o in selected)
		// 		ViewModel.DeleteFilterTask(dataContext);
		FilterTaskListView.ItemsSource = ViewModel.FilterTasks;
	}

	private void FilterTaskListView_OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (!(e.NewSize.Height > FilterTaskListView.Height * 1.02) &&
			!(e.NewSize.Height < FilterTaskListView.Height * 0.98)) return;
		FilterTaskListView.Height = e.NewSize.Height;
		// FilterResultTabView.Height = e.NewSize.Height * 1.1;
	}

	// private void TaskViewMultiSelect_OnClick(object sender, RoutedEventArgs e)
	// {
	// 	FilterTaskListView.SelectionMode = FilterTaskListView.SelectionMode == ListViewSelectionMode.Extended
	// 		                                   ? ListViewSelectionMode.Multiple
	// 		                                   : ListViewSelectionMode.Extended;
	// }
}