using System.Linq.Dynamic.Core;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using CommunityToolkit.WinUI.UI.Controls;
using Mapster;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SAaP.ControlPages;
using SAaP.Core.Models;
using SAaP.Core.Models.Analyst;
using SAaP.ViewModels;

namespace SAaP.Views;

/// <summary>
///     main page
/// </summary>
public sealed partial class MainPage
{
	private const double Offset = -7.0;
	private const double Distance = 1.05;

	public MainPage()
	{
		ViewModel = App.GetService<MainViewModel>();
		InitializeComponent();
	}

	public MainViewModel ViewModel { get; }

	public List<double> LastingDaysTemplate { get; } = new()
	{
#if DEBUG
		10, 15, 20, 30, 40, -12
#else
        5, 10, 15, 20, 30, 40, 50, 100, 120, 150, 200
#endif
	};

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		try
		{
			// restore query history from db
			await ViewModel.RestoreActivityAsync();
			// restore query history from db
			await ViewModel.RestoreLastQueryStringAsync();
			// restore favorite codes from db
			await ViewModel.RestoreFavoriteGroupsAsync();
		}
		catch (Exception exception)
		{
			Console.WriteLine(exception);
		}
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

	private void DataGrid_OnSorting2(object sender, DataGridColumnEventArgs e)
	{
		// just for sure
		if (e.Column.Tag == null) return;

		// args for linq dynamic sorting
		string args;

		foreach (var column in AnalyzeResult2Grid.Columns)
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
		AnalyzeResult2Grid.ItemsSource = ViewModel.AnalyzedResults2.AsQueryable().OrderBy(args);
	}

	private async void LastingDays_OnTextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
	{
		// clear data grid first
		AnalyzeResultGrid.ItemsSource = null;
		// execute analyze
		await ViewModel.OnLastingDaysValueChangedAsync();
		// bind item source back
		AnalyzeResultGrid.ItemsSource = ViewModel.AnalyzedResults;
	}

	private void ClearGrid_OnClick(object sender, RoutedEventArgs e)
	{
		// clear data grid
		AnalyzeResultGrid.ItemsSource = null;
	}

	private async void OnCodeInputLostFocusEventHandler(object sender, RoutedEventArgs e)
	{
		CodeInput.IsEnabled = false;
		await ViewModel.FormatCodeInput(CodeInput.Text);
		CodeInput.IsEnabled = true;
	}

	private void QueryAll_OnChecked(object sender, RoutedEventArgs e)
	{
		CodeInput.IsEnabled = false;
	}

	private void QueryUs_OnChecked(object sender, RoutedEventArgs e)
	{
	}

	private void QueryAll_OnUnchecked(object sender, RoutedEventArgs e)
	{
		CodeInput.IsEnabled = true;
	}

	private async void AddToFavoriteGroup_OnClick(object sender, RoutedEventArgs e)
	{
		var dia = new AddFavoriteGroupDialog(ViewModel.FavoriteGroups.ToList());

		var dialog = new ContentDialog
		{
			// XamlRoot must be set in the case of a ContentDialog running in a Desktop app
			XamlRoot = XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = "是这个吗？",
			PrimaryButtonText = "加入自选组",
			CloseButtonText = "取消",
			DefaultButton = ContentDialogButton.Primary,
			Content = dia
		};
		// show dialog
		var result = await dialog.ShowAsync();
		// return if non primary button clicked
		if (result != ContentDialogResult.Primary) return;
		// acquire selected/custom group name
		string groupName;
		// new?
		if (dia.CreateNewChecked)
		{
			// don't left it blank dude
			if (string.IsNullOrEmpty(dia.GroupName))
			{
				dialog.Title = "名称不可以不填<_<";
				await dialog.ShowAsync();
				return;
			}

			// gotcha
			groupName = dia.GroupName;
		}
		else
		{
			// selected groupName
			groupName = dia.GroupNames[dia.FavoriteListSelectSelectIndex];
		}

		var codes = string.Empty;

		if (sender.GetType() == typeof(DataGrid))
		{
			var grid = sender as DataGrid;

			try
			{
				var cn = ((AnalysisResult)grid!.SelectedItem).CodeName;
				codes = cn;
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}

			try
			{
				var cn = ((Report)grid!.SelectedItem).CodeName;
				codes = cn;
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}
		}
		else
		{
			codes = CodeInput.Text;
		}

		// store into db main
		await ViewModel.AddToFavoriteAsync(groupName, codes);
	}

	private void ManageGroupSelectAll_OnChecked(object sender, RoutedEventArgs e)
	{
		ManageGroupListView.SelectAll();
	}

	private void ManageGroupSelectAll_OnUnchecked(object sender, RoutedEventArgs e)
	{
		ManageGroupListView.SelectedValue = false;
	}

	private void FavoriteCodeManageSelectAll_OnChecked(object sender, RoutedEventArgs e)
	{
		FavoriteCodes.SelectAll();
	}

	private void FavoriteCodeManageSelectAll_OnUnchecked(object sender, RoutedEventArgs e)
	{
		FavoriteCodes.SelectedValue = false;
	}

	private void EditFavoriteGroup_OnClick(object sender, RoutedEventArgs e)
	{
		FavoriteCodeManagePanel.Visibility = Visibility.Visible;
		FavoriteCodeManageCancel.Visibility = Visibility.Visible;

		ManageFavoriteGroup.Visibility = Visibility.Collapsed;
		EditFavoriteGroup.Visibility = Visibility.Collapsed;

		FavoriteCodes.SelectionMode = ListViewSelectionMode.Multiple;
	}

	private void FavoriteCodeManageCancel_OnClick(object sender, RoutedEventArgs e)
	{
		FavoriteCodeManagePanel.Visibility = Visibility.Collapsed;
		FavoriteCodeManageCancel.Visibility = Visibility.Collapsed;

		ManageFavoriteGroup.Visibility = Visibility.Visible;
		EditFavoriteGroup.Visibility = Visibility.Visible;

		FavoriteCodes.SelectionMode = ListViewSelectionMode.Single;

		FavoriteCodeManageSelectAll.IsChecked = false;
	}

	private void ManageActivitySelectAll_OnChecked(object sender, RoutedEventArgs e)
	{
		ManageActivityListView.SelectAll();
	}

	private void ManageActivitySelectAll_OnUnchecked(object sender, RoutedEventArgs e)
	{
		ManageActivityListView.SelectedValue = false;
	}

	private void FavoriteListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
	{
		ViewModel.AddToQuerying(FavoriteCodes);
	}

	private void ClearInput_OnClick(object sender, RoutedEventArgs e)
	{
		ViewModel.CodeInput = "";
	}
	//
	// private void ShellMenuBarSettingsButton_PointerEntered(object sender, PointerRoutedEventArgs e)
	// {
	// 	AnimatedIcon.SetState(SettingsAppBarButton, "PointerOver");
	// }
	//
	// private void ShellMenuBarSettingsButton_PointerExited(object sender, PointerRoutedEventArgs e)
	// {
	// 	AnimatedIcon.SetState(SettingsAppBarButton, "Normal");
	// }

	private void CodeNameCell_OnClick(object sender, RoutedEventArgs e)
	{
		var hyb = sender as HyperlinkButton;
		if (hyb != null) ViewModel.RedirectToAnalyzeDetailCommand.Execute(hyb.Content);
	}

	private void DeleteFavoriteCodesButton_OnClick(object sender, RoutedEventArgs e)
	{
		DeleteFavoriteCodesButton.Flyout.Hide();
		//FavoriteCodeManageSelectAll.IsChecked = false;
	}

	private void FavoriteGroups_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		FavoriteCodeManageSelectAll.IsChecked = false;

		ViewModel.FavoriteListSelectionChanged(sender, e);
	}

	private void ExecBtn_OnClick(object sender, RoutedEventArgs e)
	{
		AnalyzeResultGrid.Visibility = Visibility.Visible;
		AnalyzeResult2Grid.Visibility = Visibility.Collapsed;
		AnalyzeResultGrid.ItemsSource = ViewModel.AnalyzedResults;
	}

	private void SpecBtn_OnClick(object sender, RoutedEventArgs e)
	{
		AnalyzeResultGrid.Visibility = Visibility.Collapsed;
		AnalyzeResult2Grid.Visibility = Visibility.Visible;
		AnalyzeResult2Grid.ItemsSource = ViewModel.AnalyzedResults2;
	}

	private void SfPanel_OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		//
		// if (e.NewSize.Height > ActivityListView.Height * Distance)
		// 	ActivityListView.Height = InfoBarA.Height + CodeInput.Height + ManageQueryHistory.Height;
		// else if (ActivityListView.Height > e.NewSize.Height * Distance)
		// 	ActivityListView.Height = InfoBarA.Height + CodeInput.Height + ManageQueryHistory.Height;
		// else if (SfPanel.DesiredSize.Height > ActivityListView.Height * Distance)
		// 	ActivityListView.Height = InfoBarA.Height + CodeInput.Height + ManageQueryHistory.Height;
		// else if (ActivityListView.Height > SfPanel.DesiredSize.Height * Distance)
		// 	ActivityListView.Height = InfoBarA.Height + CodeInput.Height + ManageQueryHistory.Height;
		// return;
		if (e.NewSize.Height > ActivityListView.Height * Distance)
			ActivityListView.Height = e.NewSize.Height * .8 + Offset;
		else if (ActivityListView.Height > e.NewSize.Height * Distance)
			ActivityListView.Height = e.NewSize.Height * .8 + Offset;
		else if (SfPanel.DesiredSize.Height > ActivityListView.Height * Distance)
			ActivityListView.Height = SfPanel.DesiredSize.Height * .8 + Offset;
		else if (ActivityListView.Height > SfPanel.DesiredSize.Height * Distance)
			ActivityListView.Height = SfPanel.DesiredSize.Height * .8 + Offset;
	}

	private void DataGridMenuFlyOutItemCopy_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			var sb = new StringBuilder();

			var selectedItems = AnalyzeResult2Grid.SelectedItems.Adapt<IList<object>>();

			for (var index = 0; index < selectedItems.Count; index++)
			{
				var t = (Report)selectedItems[index];
				if (t != null)
					sb.Append(t.CodeName[1..]).Append(' ');
			}

			ViewModel.CodeInput = sb.ToString();

			// var dataPackage = new DataPackage
			// {
			// 	RequestedOperation = DataPackageOperation.Copy
			// };
			//
			// dataPackage.SetText(ViewModel.CodeInput);
			// Clipboard.SetContent(dataPackage);
		}
		catch (Exception)
		{
			Console.WriteLine();
		}
	}
}