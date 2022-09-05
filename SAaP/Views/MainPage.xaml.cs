using Microsoft.UI.Xaml.Controls;
using SAaP.ViewModels;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;
using SAaP.Core.Models;
using System.Linq.Dynamic.Core;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Contracts.Services;
using SAaP.Services;

namespace SAaP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

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
            if (e.Column.Tag == null) return;

            // args for linq dynamic sorting
            string args;

            foreach (var column in DataGrid.Columns)
            {
                // self
                if (e.Column == column) continue;
                // clear sort direction
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
            DataGrid.ItemsSource = ViewModel.AnalyzedResults.AsQueryable().OrderBy(args);
        }

    }
}
