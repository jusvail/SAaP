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
    }
}
