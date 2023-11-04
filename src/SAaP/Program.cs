using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WinRT;

namespace SAaP;

public static class Program
{
    [STAThread]
    [DllImport("Microsoft.ui.xaml.dll")]
    private static extern void XamlCheckProcessRequirements();

#if DISABLE_XAML_GENERATED_MAIN
    [STAThread]
    private static void Main(string[] args)
    {
        if (DecideRedirection()) return;

        XamlCheckProcessRequirements();

        ComWrappersSupport.InitializeComWrappers();

        try
        {
            Application.Start(p =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                var app = new App();
				app.UnhandledException += App_UnhandledException;
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);Console.WriteLine(nameof(Program));
            App.Logger.Log(e.Message).Wait();
            App.Logger.Log(e.ToString()).Wait();
            if (e.InnerException != null) App.Logger.Log(e.InnerException.Message).Wait();
            throw;
        }
    }

	private static void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{		
		Console.WriteLine("---------------------------------------");
		Console.WriteLine("UnHandled Error happened");
		Console.WriteLine(sender);
		Console.WriteLine(e.Exception);
		Console.WriteLine(e.Message);
		Console.WriteLine("---------------------------------------");
	}
#endif

	private static bool DecideRedirection()
    {
        var isRedirect = false;

        var keyInstance = AppInstance.FindOrRegisterForKey("randomKey");

        if (keyInstance.IsCurrent)
            keyInstance.Activated += OnActivated;
        else
            isRedirect = true;

        return isRedirect;
    }

    private static void OnActivated(object sender, AppActivationArguments args)
    {
    }
}