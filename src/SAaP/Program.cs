using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

namespace SAaP;

internal class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        var isRedirect = await DecideRedirection();
        if (!isRedirect)
        {
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                // Single instance App
                _ = new App();
            });
        }
    }

    private static async Task<bool> DecideRedirection()
    {
        var isRedirect = false;
        var args = AppInstance.GetCurrent().GetActivatedEventArgs();

        var keyInstance = AppInstance.FindOrRegisterForKey("randomKey");

        if (keyInstance.IsCurrent)
        {
            keyInstance.Activated += OnActivated;
        }
        else
        {
            isRedirect = true;
            await keyInstance.RedirectActivationToAsync(args);
        }

        return isRedirect;
    }

    private static void OnActivated(object sender, AppActivationArguments args)
    {
    }
}