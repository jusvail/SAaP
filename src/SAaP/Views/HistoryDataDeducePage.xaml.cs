using Microsoft.UI.Xaml.Navigation;
using SAaP.Models;

namespace SAaP.Views;

public sealed partial class HistoryDataDeducePage
{
    public HistoryDataDeducePage()
    {
        this.InitializeComponent();
    }

    private ObservableMonitorDetail _detail;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not ObservableMonitorDetail detail) return;

        _detail = detail;
    }
}
