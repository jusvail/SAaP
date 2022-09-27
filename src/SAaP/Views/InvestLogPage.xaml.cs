using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SAaP.ControlPages;
using SAaP.Helper;
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
            UiInvokeHelper.Invoke(NewLogHiddenButton);

        }

    }
}
