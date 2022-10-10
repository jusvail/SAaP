using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.UI.Xaml;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;
using Windows.ApplicationModel.DataTransfer;
using SAaP.Contracts.Services;
using ColorCode.Common;
using SAaP.Extensions;
using SAaP.Services;
using SAaP.ViewModels;

namespace SAaP.Views;

public sealed partial class FilterResultViewerPage
{
    public ObservableCollection<Stock> Stocks { get; set; } = new();

    public FilterResultViewerPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not IEnumerable<Stock> stocks) return;

        foreach (var stock in stocks)
        {
            Stocks.Add(stock);
        }
    }

    private void StockOnly_OnToggled(object sender, RoutedEventArgs e)
    {
        if (((ToggleSwitch)sender).IsOn)
        {
            var itemSource = new ObservableCollection<Stock>();

            var filtered = Stocks.Where(s => StockService.IsStock(s.CodeName) || StockService.IsZz(s.CodeName));

            foreach (var stock in filtered)
            {
                itemSource.Add(stock);
            }

            StockGridView.ItemsSource = itemSource;
        }
        else
        {
            StockGridView.ItemsSource = Stocks;
        }
    }

    private void SelectAll_OnChecked(object sender, RoutedEventArgs e)
    {
        StockGridView.SelectAll();
    }

    private void SelectAll_OnUnchecked(object sender, RoutedEventArgs e)
    {
        StockGridView.SelectedIndex = -1;
    }

    private async void Copy_OnClick(object sender, RoutedEventArgs e)
    {
        var dataPackage = new DataPackage
        {
            RequestedOperation = DataPackageOperation.Copy
        };

        var sb = new StringBuilder();

        var button = (Button)sender;

        try
        {
            for (var index = 0; index < StockGridView.SelectedItems.Count; index++)
            {
                var stock = (Stock)StockGridView.SelectedItems[index];
                sb.Append(stock.CodeNameFull).Append(" ");
            }

            dataPackage.SetText(sb.ToString());
            Clipboard.SetContent(dataPackage);

        }
        catch (Exception)
        {
            button.Content = "复制出错！";
        }

        button.Content = "完成！";

        await Task.Delay(1000);

        button.Content = "复制";
    }

    private void GoToDetail_OnClick(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not Stock stock) return;

        var title = "AnalyzeDetailPageTitle".GetLocalized() + $": [{stock.CodeNameFull} {stock.CompanyName}]";

        App.GetService<IWindowManageService>().CreateOrBackToWindow<AnalyzeDetailPage>(typeof(AnalyzeDetailViewModel).FullName!, title, stock.CodeNameFull);

    }
}