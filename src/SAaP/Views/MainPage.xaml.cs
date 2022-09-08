using Microsoft.UI.Xaml.Controls;
using SAaP.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using System.Linq.Dynamic.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Core.Services;
using SAaP.Core.Helpers;
using SAaP.ControlPages;

namespace SAaP.Views
{
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
            NotifyUser.Message = "查询所有可能花费大量时间。";
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

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            string groupName;

            if (dia.CreateNewChecked)
            {
                if (string.IsNullOrEmpty(dia.GroupName))
                {
                    dialog.Title = "名称不可以不填<_<";
                    await dialog.ShowAsync();
                    return;
                }

                groupName = dia.GroupName;
            }
            else
            {
                groupName = dia.GroupNames[dia.FavoriteListSelectSelectIndex];
            }

            await ViewModel.AddToFavorite(groupName);
        }
    }
}
