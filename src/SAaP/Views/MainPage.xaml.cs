using Microsoft.UI.Xaml.Controls;
using SAaP.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using System.Linq.Dynamic.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Core.Services;
using SAaP.Core.Helpers;
using SAaP.ControlPages;
using SAaP.Core.Models;

namespace SAaP.Views;

/// <summary>
/// main page
/// </summary>
public sealed partial class MainPage
{
    public MainViewModel ViewModel { get; }

    public List<double> LastingDaysTemplate { get; } = new()
    {
        5, 10, 15, 20, 30, 40, 50, 100, 120, 150, 200
    };

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // restore query history from db
        await ViewModel.RestoreActivity();
        // restore query history from db
        await ViewModel.RestoreLastQueryString();
        // restore favorite codes from db
        await ViewModel.RestoreFavoriteGroups();
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

    private async void LastingDays_OnTextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        // clear data grid first
        AnalyzeResultGrid.ItemsSource = null;
        // execute analyze
        await ViewModel.OnLastingDaysValueChanged();
        // bind item source back
        AnalyzeResultGrid.ItemsSource = ViewModel.AnalyzedResults;
    }

    private void ClearGrid_OnClick(object sender, RoutedEventArgs e)
    {
        // clear data grid
        AnalyzeResultGrid.ItemsSource = null;
    }

    private void OnCodeInputLostFocusEventHandler(object sender, RoutedEventArgs e)
    {
        // return if is null
        if (string.IsNullOrEmpty(CodeInput.Text)) return;

        var formatted = StringHelper.FormatInputCode(CodeInput.Text);

        if (formatted != null)
            // format input code
            CodeInput.Text = StockService.FormatPyArgument(formatted);
    }

    private void QueryAll_OnChecked(object sender, RoutedEventArgs e)
    {
        CodeInput.IsEnabled = false;
        NotifyUser.IsEnabled = true;
        NotifyUser.IsOpen = true;
        NotifyUser.Title = "警告";
        NotifyUser.Message = "查询所有股票可能会花费大量时间。(几小时😎, 甚至几天)";
    }

    private void QueryAll_OnUnchecked(object sender, RoutedEventArgs e)
    {
        CodeInput.IsEnabled = true;
        NotifyUser.IsOpen = false;
        NotifyUser.IsEnabled = false;
    }

    private async void AddToFavoriteGroup_OnClick(object sender, RoutedEventArgs e)
    {
        var dia = new AddFavoriteGroupDialog(ViewModel.FavoriteGroups.ToList());
        // 把逻辑写在这里真的很丑陋<_<
        // 有没有更好的办法？
        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = this.XamlRoot,
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

        string codes;

        if (sender.GetType() == typeof(DataGrid))
        {
            var grid = sender as DataGrid;

            var analysis = (AnalysisResult)grid!.SelectedItem;

            codes = analysis.CodeName;
        }
        else
        {
            codes = CodeInput.Text;
        }

        // store into db main
        await ViewModel.AddToFavorite(groupName, codes);
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

    private void ShellMenuBarSettingsButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "PointerOver");
    }

    private void ShellMenuBarSettingsButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void CodeNameCell_OnClick(object sender, RoutedEventArgs e)
    {
        var hyb = sender as HyperlinkButton;
        if (hyb != null) ViewModel.RedirectToAnalyzeDetailCommand.Execute(hyb.Content);
    }

    private void DeleteFavoriteCodesButton_OnClick(object sender, RoutedEventArgs e)
    {
        FavoriteCodeManageSelectAll.IsChecked = false;
    }

    private void FavoriteGroups_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FavoriteCodeManageSelectAll.IsChecked = false;

        ViewModel.FavoriteListSelectionChanged(sender, e);
    }
}