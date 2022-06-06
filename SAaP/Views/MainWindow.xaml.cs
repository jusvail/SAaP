using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SAaP.Views
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class MainWindow : Page
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";

        }
    }
}
