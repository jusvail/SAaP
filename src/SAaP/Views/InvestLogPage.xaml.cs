using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using SAaP.ViewModels;

namespace SAaP.Views
{
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

        private void InvestLogAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            var peer = new ButtonAutomationPeer(NewLogHiddenButton);
            var provider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

            provider?.Invoke();
        }
    }
}
