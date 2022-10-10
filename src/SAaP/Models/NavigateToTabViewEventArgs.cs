using Microsoft.UI.Xaml.Controls;
using SAaP.Core.Models.DB;

namespace SAaP.Models;

public class NavigateToTabViewEventArgs : EventArgs
{
    public string TaskName { get; set; }

    public IEnumerable<Stock> Stocks { get; set; }

    public TabView Target { get; set; }
}