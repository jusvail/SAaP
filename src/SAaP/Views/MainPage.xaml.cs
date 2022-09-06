using Microsoft.UI.Xaml.Controls;
using SAaP.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using System.Linq.Dynamic.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Contracts.Services;
using SAaP.Core.Services;
using SAaP.Core.Helpers;

namespace SAaP.Views
{
    /// <summary>
    /// main page
    /// </summary>
    public sealed partial class MainPage
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

            // restore query history from db
            await ViewModel.RestoreLastQueryString();

            // restore favorite codes from db
            await ViewModel.RestoreFavoriteCodesString();
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
    }
}
