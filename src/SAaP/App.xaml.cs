﻿using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SAaP.Chart.Contracts.Services;
using SAaP.Contracts.Services;
using SAaP.Core.Contracts.Services;
using SAaP.Core.Services;
using SAaP.Extensions;
using SAaP.Models;
using SAaP.Services;
using SAaP.Views;
using SAaP.ViewModels;
using SAaP.Chart.Services;

namespace SAaP;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App
{
    public IHost Host { get; }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static Window MainWindow { get; set; } = new() { Title = "AppTitle".GetLocalized() };

    public static string MainWindowKey = nameof(MainWindow);

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
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

                    services.AddSingleton<IFileService, FileService>();

                    services.AddTransient<AnalyzeDetailPage>();
                    services.AddTransient<AnalyzeDetailViewModel>();
                    services.AddTransient<MainPage>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<ShellPage>();
                    services.AddTransient<ShellViewModel>();
                    services.AddTransient<SettingsPage>();
                    services.AddTransient<SettingsViewModel>();

                    // Configuration
                    services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
                }
            ).Build();

        // register unhandled exception
        UnhandledException += App_UnhandledException;
    }

    private static void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Console.Write(e.Message);
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await GetService<IActivationService>().ActivateAsync(args);
        GetService<IWindowManageService>().TrackWindow(MainWindow, MainWindowKey);
    }
}