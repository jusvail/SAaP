using System.Reflection;
using Windows.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using SAaP.Contracts.Services;
using SAaP.Core.Contracts.Services;
using SAaP.Core.Services.Generic;
using SAaP.Models;
using SAaP.Services;
using SAaP.ViewModels;
using SAaP.Views;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

namespace SAaP;

/// <summary>
///     Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App
{
	/// <summary>
	///     Initializes the singleton application object.  This is the first line of authored code
	///     executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		InitializeComponent();

		Host = Microsoft.Extensions.Hosting.Host
		                .CreateDefaultBuilder()
		                .UseContentRoot(AppContext.BaseDirectory)
		                .ConfigureServices((context, services) =>
			                {
				                services.AddSingleton<IActivationService, ActivationService>();
				                services.AddSingleton<IDbTransferService, DbTransferService>();
				                services.AddSingleton<IFetchStockDataService, FetchStockDataService>();
				                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
				                services.AddSingleton<IPageService, PageService>();
				                services.AddSingleton<IRestoreSettingsService, RestoreSettingsService>();
				                services.AddSingleton<IStockAnalyzeService, StockAnalyzeService>();
				                services.AddSingleton<IWindowManageService, WindowManageService>();
				                services.AddSingleton<IChartService, ChartService>();
				                services.AddSingleton<IMonitorService, MonitorService>();
				                services.AddSingleton<ILogger, Logger>();

				                services.AddSingleton<IFileService, FileService>();

				                services.AddTransient<AnalyzeDetailPage>();
				                services.AddTransient<AnalyzeDetailViewModel>();
				                services.AddTransient<MainPage>();
				                services.AddTransient<MainViewModel>();
				                services.AddTransient<SettingsPage>();
				                services.AddTransient<SettingsViewModel>();
				                services.AddTransient<InvestLogPage>();
				                services.AddTransient<InvestLogViewModel>();
				                services.AddTransient<MonitorPage>();
				                services.AddTransient<MonitorViewModel>();
				                services.AddTransient<SimulatePage>();
				                services.AddTransient<SimulatePageViewModel>();
				                services.AddTransient<StatisticsPage>();
				                services.AddTransient<StatisticsViewModel>();

				                services.AddTransient<ObservableInvestDetail>();

				                // Configuration
				                services.Configure<LocalSettingsOptions>(
					                context.Configuration.GetSection(nameof(LocalSettingsOptions)));
			                }
		                ).Build();

		// register unhandled exception
		UnhandledException += App_UnhandledException;
	}

	public IHost Host { get; }

	public static Window MainWindow { get; set; }

	public string Version { get; set; }

	public static ILogger Logger { get; set; }

	public static bool OnGoingTask { get; set; }

	public static T GetService<T>()
		where T : class
	{
		if ((Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
			throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");

		return service;
	}

	public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
	{
		if (!typeof(TEnum).GetTypeInfo().IsEnum)
			throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
		return (TEnum)Enum.Parse(typeof(TEnum), text);
	}

	private static async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Console.Write(e.Message);
		await Logger.Log(e.ToString());
		await Logger.Log(e.Exception.ToString());
		if (e.Exception.InnerException != null) await Logger.Log(e.Exception.InnerException.StackTrace);
		await Logger.Log(e.Exception.StackTrace);
	}

	/// <summary>
	///     Invoked when the application is launched normally by the end user.  Other entry points
	///     will be used such as when the application is launched to open a specific file.
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		base.OnLaunched(args);

		Logger = GetService<ILogger>();

		MainWindow                            = GetService<IWindowManageService>().CreateWindow(typeof(NavigationRootPage).FullName!);
		MainWindow.ExtendsContentIntoTitleBar = true;

		GetRootFrame();

		var rootPage = MainWindow.Content as NavigationRootPage;

		if (rootPage == null) throw new ApplicationException("rootPage not found");

		rootPage.Navigate(typeof(MainPage));

#if STATISTICS
		rootPage.Navigate(typeof(StatisticsPage));
#endif

		await GetService<IActivationService>().ActivateAsync(args);
		//
		// var notificationManager = AppNotificationManager.Default;
		// notificationManager.NotificationInvoked += NotificationManager_NotificationInvoked;
		// notificationManager.Register();
		//
		// var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
		// var activationKind = activatedArgs.Kind;
		// if (activationKind != ExtendedActivationKind.AppNotification)
		// {
		//     App.GetService<IWindowManageService>().SetWindowForeground(NavigationRootPage);
		// }
		// else
		// {
		//     HandleNotification((AppNotificationActivatedEventArgs)activatedArgs.Data);
		// }
	}


	public Frame GetRootFrame()
	{
		Frame rootFrame;
		var   rootPage = MainWindow.Content as NavigationRootPage;
		if (rootPage == null)
		{
			rootPage  = new NavigationRootPage();
			rootFrame = (Frame)rootPage.FindName("RootFrame");
			if (rootFrame == null) throw new Exception("Root frame not found");
			rootFrame.Language         =  ApplicationLanguages.Languages[0];
			rootFrame.NavigationFailed += OnNavigationFailed;

			MainWindow.Content = rootPage;
		}
		else
		{
			rootFrame = (Frame)rootPage.FindName("RootFrame");
		}

		return rootFrame;
	}

	private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
	{
		throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
	}

	private void NotificationManager_NotificationInvoked(AppNotificationManager sender,
	                                                     AppNotificationActivatedEventArgs args)
	{
		//throw new NotImplementedException();
	}
}