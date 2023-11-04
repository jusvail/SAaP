using Microsoft.UI.Xaml.Input;
using SAaP.Helper;
using SAaP.Models;
using Mapster;

namespace SAaP.ControlPages;

public sealed partial class TradeListRow
{
    private readonly ObservableInvestDetail _investDetail = new();

    public ObservableInvestDetail InvestDetail
    {
        get => _investDetail;
        set => value.Adapt(_investDetail);
    }

    public TradeListRow()
    {
        InvestDetail = App.GetService<ObservableInvestDetail>();
        this.InitializeComponent();
    }

    private void TradeListRow_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (!InvestDetail.Editable) return;

        UiInvokeHelper.TriggerButtonPressed(HdFlyOutButton);
        AddToTradeListDialog.InvestDetail = InvestDetail;
    }
}