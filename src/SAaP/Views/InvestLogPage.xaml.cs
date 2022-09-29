using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Core.Models.DB;
using SAaP.Helper;
using SAaP.ViewModels;

namespace SAaP.Views;

/// <summary>
/// Invest Log Page
/// </summary>
public sealed partial class InvestLogPage
{
    public InvestLogViewModel ViewModel { get; }

    public InvestLogPage()
    {
        ViewModel = App.GetService<InvestLogViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitialInvestSummaryDetail();
        await ViewModel.InitialFirstSummaryDetail();
        await ViewModel.RefreshReminder();
    }

    private void BuyListView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ViewModel.ClickedRowsOriginalTradeType = TradeType.Buy;
    }
    private void SellListView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ViewModel.ClickedRowsOriginalTradeType = TradeType.Sell;
    }

    private async void SoldAllToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (!SoldAllToggleSwitch.IsOn || ViewModel.CheckIfSoldAll()) return;

        await Task.Delay(200);
        SoldAllToggleSwitch.IsOn = false;
    }

    private void NewSummaryRecordButtonConfirm_OnClick(object sender, RoutedEventArgs e)
    {
        NewSummaryRecordButton.Flyout.Hide();
    }

    private async void ReminderBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter) return;

        var box = sender as TextBox;
        if (box == null) return;

        if (string.IsNullOrEmpty(box.Text)) return;

        NewLogHiddenButton.Flyout.Hide();
        await ViewModel.AddNewReminderCommand(sender, e);
    }

    private void InvestLogAppBarButton_OnClick(object sender, RoutedEventArgs e)
    {
        UiInvokeHelper.Invoke(NewLogHiddenButton);
        ViewModel.ReminderSelectedIndex = -1;
        ViewModel.ReminderContent = string.Empty;
    }

    private void Reminder_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        UiInvokeHelper.Invoke(NewLogHiddenButton);
        ViewModel.ReminderOnDoubleTapped(sender, e);
    }
}