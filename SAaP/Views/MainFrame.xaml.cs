using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Constant;
using SAaP.Extensions;

namespace SAaP.Views
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class MainFrame : Page
    {
        public MainFrame()
        {
            InitializeComponent();

            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.SetTitleBar(AppTitleBar);
            AppTitleBarText.Text = PjConstant.AppTitle.GetLocalized();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

    }
}
