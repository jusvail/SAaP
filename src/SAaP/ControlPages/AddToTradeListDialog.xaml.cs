using Microsoft.UI.Xaml.Controls;
using SAaP.Core.Models;

namespace SAaP.ControlPages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddToTradeListDialog
{
    public InvestDetail InvestDetail { get; set; }

    public AddToTradeListDialog()
    {
        this.InitializeComponent();
    }

    public AddToTradeListDialog(InvestDetail detail) : this()
    {
        InvestDetail = detail;
    }


}