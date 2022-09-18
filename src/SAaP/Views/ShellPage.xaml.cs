using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.ViewModels;

namespace SAaP.Views
{
    /// <summary>
    /// main shell page
    /// </summary>
    public sealed partial class ShellPage
    {
        public ShellViewModel ViewModel { get; }

        public ShellPage()
        {
            ViewModel = App.GetService<ShellViewModel>();
            InitializeComponent();
        }

        public void ReadyToNavigate<T>(Window window, string title, object arg) where T : Page
        {
            window.ExtendsContentIntoTitleBar = true;
            window.SetTitleBar(AppTitleBar);
            AppTitleBarText.Text = title;
            // frame navigate to
            NavigationFrame.Navigate(typeof(T), arg);
        }
    }
}