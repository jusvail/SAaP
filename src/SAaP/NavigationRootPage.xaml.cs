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

		AppTitle.Text    =  PjConstant.AppTitle.GetLocalized();
		window.Title     =  PjConstant.AppTitle.GetLocalized();
		window.Activated += Window_Activated;
		window.SetTitleBar(AppTitleBar);
	}

	private void Window_Activated(object sender, WindowActivatedEventArgs args)
	{
		VisualStateManager.GoToState(this, args.WindowActivationState == WindowActivationState.Deactivated
			                                   ? "Deactivated"
			                                   : "Activated", true);
	}

	private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		if (args.IsSettingsSelected)
		{
			if (RootFrame.CurrentSourcePageType != typeof(SettingsPage)) Navigate(typeof(SettingsPage));
		}
		else
		{
			var selectedItem = args.SelectedItemContainer;
			if (selectedItem == MainPage)
			{
				if (RootFrame.CurrentSourcePageType != typeof(MainPage))
					Navigate(typeof(MainPage));
			}
			else if (selectedItem == Filter)
			{
				if (RootFrame.CurrentSourcePageType != typeof(MonitorPage))
					Navigate(typeof(MonitorPage));
			}
			else if (selectedItem == Statistics)
			{
				if (RootFrame.CurrentSourcePageType != typeof(StatisticsPage))
					Navigate(typeof(StatisticsPage));
			}
			else if (selectedItem == Log)
			{
				if (RootFrame.CurrentSourcePageType != typeof(InvestLogPage))
					Navigate(typeof(InvestLogPage));
			}
		}
	}

	public void Navigate(
		Type pageType,
		object targetPageArguments = null,
		NavigationTransitionInfo navigationTransitionInfo = null)
	{
		var args = new NavigationRootPageArgs
		{
			NavigationRootPage = this,
			Parameter          = targetPageArguments
		};
		RootFrame.Navigate(pageType, args, navigationTransitionInfo);
	}

	private void NavigationViewControl_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
	{
		if (!RootFrame.CanGoBack) return;

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
	public object             Parameter;
}