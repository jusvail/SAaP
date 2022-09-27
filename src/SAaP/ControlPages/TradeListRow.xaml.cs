using Microsoft.UI.Xaml.Input;
using SAaP.Core.Models.DB;
using SAaP.Helper;
using SAaP.Models;
using System.Windows.Input;
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

    public ICommand ConfirmCommand { get; set; }

    private void TradeListRow_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        AddToTradeListDialog.ConfirmCommand = ConfirmCommand;
        UiInvokeHelper.Invoke(HdFlyOutButton);
        AddToTradeListDialog.InvestDetail = InvestDetail;
    }
}