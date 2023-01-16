using Microsoft.UI.Xaml.Navigation;
using SAaP.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SAaP.Core.Models.DB;
using SAaP.Helper;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using SAaP.Core.Models.Monitor;

namespace SAaP.Views;

public sealed partial class MonitorPage
{
    public MonitorViewModel ViewModel { get; }

    public MonitorPage()
    {
        ViewModel = App.GetService<MonitorViewModel>();

        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await ViewModel.InitializeSuggestData();
        await ViewModel.InitializeTrackData();
        await ViewModel.InitializeMonitorStockData();

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

    private async void CodeSelectSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Stock stock)
        {
            await ViewModel.AddToMonitor(stock);
        }
        else if (!string.IsNullOrEmpty(args.QueryText))
        {
            await ViewModel.AddToMonitor(args.QueryText);
        }
        sender.Text = string.Empty;
    }

    private async void CodeSelectSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        await ViewModel.AddToMonitor(args.SelectedItem);
        sender.Text = string.Empty;
    }

    private  void DeleteMonitor_OnClick(object sender, RoutedEventArgs e)
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
        ViewModel.CurrentTrackFilterCondition.IsValid = false;
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
        foreach (var buyMode in ViewModel.CurrentMonitorData.BuyModes)
        {
            buyMode.IsChecked = true;
        }
    }

    private void BuyModeSelect_OnUnchecked(object sender, RoutedEventArgs e)
    {
        foreach (var buyMode in ViewModel.CurrentMonitorData.BuyModes)
        {
            buyMode.IsChecked = false;
        }
    }

    private void ImportConfirm_OnClick(object sender, RoutedEventArgs e)
    {
        ReadyToImportButton.Flyout.Hide();
    }

    private void MonitorSelectAll_OnClick(object sender, RoutedEventArgs e)
    {
        if (MonitorStocksListView.SelectedItems.Count == MonitorStocksListView.Items.Count)
        {
            MonitorStocksListView.SelectedValue = false;
        }
        else
        {
            MonitorStocksListView.SelectAll();
        }
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
            {
                sb.Append(t.FullTime.ToString("yyyy/MM/dd HH:mm:ss")).Append("\t")
                    .Append(t.CodeName).Append("\t")
                    .Append(t.CompanyName).Append("\t")
                    .Append(t.Direction).Append("\t")
                    .Append(t.Price).Append("\t")
                    .Append(BuyMode.ModeDetails[t.SubmittedByMode]).Append("\t")
                    .Append(t.Message).Append("\t")
                    .Append(Environment.NewLine);
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

        button.Content = "复制结果";
    }
}