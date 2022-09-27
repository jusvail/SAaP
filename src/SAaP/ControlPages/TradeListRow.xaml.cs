using Microsoft.UI.Xaml.Input;
using SAaP.Helper;
using SAaP.Models;
using Mapster;
using CommunityToolkit.Mvvm.Input;

namespace SAaP.ControlPages;

public sealed partial class TradeListRow
{
    private readonly ObservableInvestDetail _investDetail = new();

    public ObservableInvestDetail InvestDetail
    {
        get => _investDetail;
        set => value.Adapt(_investDetail);
    }

    public IRelayCommand<object> ConfirmCommand { get; set; }
    public IRelayCommand<object> DeleteCommand { get; set; }

    public TradeListRow()
    {
        InvestDetail = App.GetService<ObservableInvestDetail>();
        this.InitializeComponent();
    }

    private void TradeListRow_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        UiInvokeHelper.Invoke(HdFlyOutButton);
        AddToTradeListDialog.InvestDetail = InvestDetail;
    }
}