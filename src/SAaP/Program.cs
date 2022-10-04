using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

namespace SAaP;

public static class Program
{
    [STAThread]
    [System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
    private static extern void XamlCheckProcessRequirements();

    [STAThread]
    static void Main(string[] args)
    {
        if (DecideRedirection()) return;

        XamlCheckProcessRequirements();

        WinRT.ComWrappersSupport.InitializeComWrappers();

        try
        {
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static bool DecideRedirection()
    {
        var isRedirect = false;

        var keyInstance = AppInstance.FindOrRegisterForKey("randomKey");

        if (keyInstance.IsCurrent)
        {
            keyInstance.Activated += OnActivated;
        }
        else
        {
            isRedirect = true;
        }

        return isRedirect;
    }

    private static void OnActivated(object sender, AppActivationArguments args)
    {
    }
}