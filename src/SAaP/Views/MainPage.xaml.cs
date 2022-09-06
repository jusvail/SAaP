using Microsoft.UI.Xaml.Controls;
using SAaP.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using System.Linq.Dynamic.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Contracts.Services;

namespace SAaP.Views
{
    /// <summary>
    /// main page
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public List<double> FontSizes { get; } = new()
        {
            5, 10, 15, 20, 30, 50, 100, 120, 150, 200
        };

        public MainPage()
        {
            ViewModel = App.GetService<MainViewModel>();
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // get service
            var restoreSettingsService = App.GetService<IRestoreSettingsService>();
            // restore query history to db
            ViewModel.CodeInput = await restoreSettingsService.RestoreLastQueryStringFromDb();
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
            AnalyzeResultGrid.ItemsSource = null;
            await ViewModel.OnLastingDaysValueChanged();
            AnalyzeResultGrid.ItemsSource = ViewModel.AnalyzedResults;
        }

        private void ClearGrid_OnClick(object sender, RoutedEventArgs e)
        {
            AnalyzeResultGrid.ItemsSource = null;
        }
    }
}
