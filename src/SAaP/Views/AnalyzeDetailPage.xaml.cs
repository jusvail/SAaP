using CommunityToolkit.WinUI.UI.Controls;
using SAaP.ViewModels;
using System.Linq.Dynamic.Core;

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

    public AnalyzeDetailPage(string codeName) :this()
    {
        ViewModel.CodeName = codeName;

        ViewModel.Initialize();
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




}