using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SAaP.Constant;
using SAaP.Extensions;
using SAaP.ViewModels;

namespace SAaP.Views
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class ShellPage : Page
    {

        public ShellViewModel ViewModel { get; }

        public ShellPage()
        {
            ViewModel = App.GetService<ShellViewModel>();
            InitializeComponent();

            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.SetTitleBar(AppTitleBar);
            AppTitleBarText.Text = PjConstant.AppTitle.GetLocalized();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NavigationFrame.Navigate(typeof(MainPage));
        }
    }
}