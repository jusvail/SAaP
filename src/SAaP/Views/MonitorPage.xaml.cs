using Microsoft.UI.Xaml.Navigation;
using SAaP.ViewModels;
using Windows.ApplicationModel.Core;
using Microsoft.UI.Xaml;
using SAaP.Contracts.Services;

namespace SAaP.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MonitorPage
{
    public MonitorViewModel ViewModel { get; }


    public MonitorPage()
    {
        ViewModel = App.GetService<MonitorViewModel>();

        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

    }

    private void AddToMonitorAppBarButton_OnClick(object sender, RoutedEventArgs e)
    {
    }
}