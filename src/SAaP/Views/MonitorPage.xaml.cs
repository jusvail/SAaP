using Microsoft.UI.Xaml.Navigation;
using SAaP.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SAaP.Core.Models.DB;
using SAaP.Helper;
using LinqToDB;

namespace SAaP.Views;

public sealed partial class MonitorPage
{
    public MonitorViewModel ViewModel { get; }

    public MonitorPage()
    {
        ViewModel = App.GetService<MonitorViewModel>();

        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await ViewModel.InitializeSuggestData();
        await ViewModel.InitializeTrackData();
        await ViewModel.InitializeMonitorStockData();

        ViewModel.CurrentMonitorData.BuyModes[0].IsChecked = true;
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

    private void CodeSelectSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is Stock stock)
        {
            ViewModel.AddToMonitor(stock);
        }
        else if (!string.IsNullOrEmpty(args.QueryText))
        {
            ViewModel.AddToMonitor(args.QueryText);
        }
        sender.Text = string.Empty;
    }

    private void CodeSelectSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        ViewModel.AddToMonitor(args.SelectedItem);
        sender.Text = string.Empty;
    }

    private void DeleteMonitor_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteMonitorItem(((FrameworkElement)e.OriginalSource).DataContext);
    }

    private void DeleteHistoryDeduceData_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteHistoryDeduce(((FrameworkElement)e.OriginalSource).DataContext);
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

    private void AllStartButton_OnClick(object sender, RoutedEventArgs e)
    {
        DoNotOpenTooMuch.IsOpen = true;
    }

    private void HistoryDeduce_OnClick(object sender, RoutedEventArgs e)
    {
        var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;

        if (dataContext is not Stock stock) return;

        if (!ViewModel.HistoryDeduceData.MonitorStocks.Contains(stock))
        {
            ViewModel.HistoryDeduceData.MonitorStocks.Add(stock);
            ViewModel.ReinsertToDb(ActivityData.HistoryDeduce, ViewModel.HistoryDeduceData.MonitorStocks);
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
}