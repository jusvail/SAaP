using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Constant;
using SAaP.Extensions;

namespace SAaP.Views
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            InitializeComponent();

            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.SetTitleBar(AppTitleBar);
            AppTitleBarText.Text = PjConstant.AppTitle.GetLocalized();
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NavigationFrame.Navigate(typeof(MainPage));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}