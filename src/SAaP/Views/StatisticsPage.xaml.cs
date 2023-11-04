using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using SAaP.ViewModels;

namespace SAaP.Views
{ 
	public sealed partial class StatisticsPage
	{
		public StatisticsViewModel ViewModel { get; set; }

		public StatisticsPage()
		{
			
			ViewModel = App.GetService<StatisticsViewModel>();
			this.InitializeComponent();
		}

		private async void OnCodeInputLostFocusEventHandler(object sender, RoutedEventArgs e)
		{
			await ViewModel.FormatCodeInput(CodeInput.Text);
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.InitializeField();
			await ViewModel.RestoreLastQueryStringAsync();
		}
	}
}
