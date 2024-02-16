using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SAaP.Constant;
using SAaP.Extensions;
using SAaP.Helper;
using SAaP.Services;
using SAaP.Views;

namespace SAaP;

public sealed partial class NavigationRootPage
{
	public NavigationRootPage()
	{
		InitializeComponent();

		((FrameworkElement)Content).RequestedTheme = ThemeHelper.RootTheme;

		var wsdqHelper = new WindowsSystemDispatcherQueueHelper();
		wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

		Loaded += NavigationRootPage_Loaded;
	}

	public Action NavigationViewLoaded { get; set; }

	public bool IsPaneOpen { get; set; }

	private void NavigationRootPage_Loaded(object sender, RoutedEventArgs e)
	{
		var window = WindowManageService.GetWindowForElement(sender as UIElement, typeof(NavigationRootPage).FullName!);

		AppTitle.Text = PjConstant.AppTitle.GetLocalized();
		window.Title = PjConstant.AppTitle.GetLocalized();
		window.Activated += Window_Activated;
		window.SetTitleBar(AppTitleBar);
	}

	private void Window_Activated(object sender, WindowActivatedEventArgs args)
	{
		VisualStateManager.GoToState(this, args.WindowActivationState == WindowActivationState.Deactivated
											   ? "Deactivated"
											   : "Activated", true);
	}

	private async void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		var selectedItem = args.SelectedItemContainer;

		if (args.IsSettingsSelected && RootFrame.CurrentSourcePageType == typeof(SettingsPage)
			|| (selectedItem == MainPage && RootFrame.CurrentSourcePageType == typeof(MainPage))
			|| (selectedItem == Filter && RootFrame.CurrentSourcePageType == typeof(MonitorPage))
			|| (selectedItem == Statistics && RootFrame.CurrentSourcePageType == typeof(StatisticsPage))
			|| (selectedItem == Log && RootFrame.CurrentSourcePageType == typeof(InvestLogPage))
			) return;

		if (App.OnGoingTask)
		{
			if (await ContinueTask()) return;
		}

		if (args.IsSettingsSelected)
		{
			Navigate(typeof(SettingsPage));
		}
		else
		{
			if (selectedItem == MainPage)
			{
				Navigate(typeof(MainPage));
			}
			else if (selectedItem == Filter)
			{
				Navigate(typeof(MonitorPage));
			}
			else if (selectedItem == Statistics)
			{
				Navigate(typeof(StatisticsPage));
			}
			else if (selectedItem == Log)
			{
				Navigate(typeof(InvestLogPage));
			}
		}
	}

	private async Task<bool> ContinueTask()
	{
		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = "确认",
			PrimaryButtonText = "确认",
			CloseButtonText = "取消",
			DefaultButton = ContentDialogButton.Primary,
			Content = "有正在进行的任务，确认离开吗？"
		};
		// show dialog
		var result = await dialog.ShowAsync();

		// return if non primary button clicked
		return result != ContentDialogResult.Primary;
	}

	public void Navigate(
		Type pageType,
		object targetPageArguments = null,
		NavigationTransitionInfo navigationTransitionInfo = null)
	{
		var args = new NavigationRootPageArgs
		{
			NavigationRootPage = this,
			Parameter = targetPageArguments
		};
		RootFrame.Navigate(pageType, args, navigationTransitionInfo);
	}

	private async void NavigationViewControl_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
	{
		if (!RootFrame.CanGoBack) return;

		if (App.OnGoingTask)
		{
			if (await ContinueTask()) return;
		}

		RootFrame.GoBack();

		object item = null;
		if (RootFrame.CurrentSourcePageType == typeof(MainPage))
			item = MainPage;
		else if (RootFrame.CurrentSourcePageType == typeof(MonitorPage))
			item = Filter;
		else if (RootFrame.CurrentSourcePageType == typeof(InvestLogPage))
			item = Log;
		else if (RootFrame.CurrentSourcePageType == typeof(StatisticsPage))
			item = Statistics;
		else if (RootFrame.CurrentSourcePageType == typeof(SettingsPage))
			item = sender.SettingsItem;
		if (item != null) sender.SelectedItem = item;
	}

	private void OnNavigationViewControlLoaded(object sender, RoutedEventArgs e)
	{
		// Delay necessary to ensure NavigationView visual state can match navigation
		Task.Delay(500).ContinueWith(_ => NavigationViewLoaded?.Invoke(), TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void OnPaneDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
	{
		if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
		{
			VisualStateManager.GoToState(this, "Top", true);
		}
		else
		{
			if (args.DisplayMode == NavigationViewDisplayMode.Minimal)
				VisualStateManager.GoToState(this, "Compact", true);
			else
				VisualStateManager.GoToState(this, "Default", true);
		}
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		NavigationViewControl.SelectedItem = MainPage;
	}
}

public class NavigationRootPageArgs
{
	public NavigationRootPage NavigationRootPage;
	public object Parameter;
}