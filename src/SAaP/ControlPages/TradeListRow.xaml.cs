using SAaP.Core.Models.DB;

namespace SAaP.ControlPages;

public sealed partial class TradeListRow
{
    public DateTime TradeDate { get; set; }

    public DateTime TradeTime { get; set; }

    public TradeType TradeType { get; set; }

    public int Volume { get; set; }

    public double Price { get; set; }

    public double Amount { get; set; }

    public TradeListRow()
    {
        this.InitializeComponent();
    }
}