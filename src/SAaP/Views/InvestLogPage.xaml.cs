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
    }

    private void InvestLogAppBarButton_OnClick(object sender, RoutedEventArgs e)
    {
        UiInvokeHelper.Invoke(NewLogHiddenButton);
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

}