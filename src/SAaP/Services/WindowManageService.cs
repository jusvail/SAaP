using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Enum;
using SAaP.Contracts.Services;
using SAaP.Extensions;
using SAaP.Views;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace SAaP.Services;

public class WindowManageService : IWindowManageService
{
    private readonly IPageService _pageService;

    public Dictionary<string, List<Window>> ActiveWindows { get; } = new();

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr SetActiveWindow(IntPtr hWnd);


    public WindowManageService(IPageService pageService)
    {
        _pageService = pageService;
    }

    private static void SetWindowForeground(Window window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);

        BringWindowToTop(hwnd);
    }

    public Window CreateWindow(string key)
    {
        var newWindow = new Window();
        TrackWindow(newWindow, key);

        return newWindow;
    }

    public void CreateWindowAndNavigateTo<T>(string key, string title, object arg) where T : Page
    {
        if (!string.IsNullOrEmpty(key))
        {
            lock (ActiveWindows)
            {
                if (ActiveWindows.Keys.Contains(key)
                    && InstanceType.Single == _pageService.GetPageInstanceType(key))
                {
                    SetWindowForeground(ActiveWindows[key].First());
                    return;
                }
            }
        }

        var window = CreateWindow(key);

        var shellPage = App.GetService<ShellPage>();
        shellPage.ReadyToNavigate<T>(window, string.IsNullOrEmpty(title) ? $"{typeof(T).Name}Title".GetLocalized() : title, arg);

        window.Content = shellPage;

        window.Activate();

        var hwnd = WindowNative.GetWindowHandle(window);
        SetActiveWindow(hwnd);

        var context = new DispatcherQueueSynchronizationContext(window.DispatcherQueue);

        SynchronizationContext.SetSynchronizationContext(context);
    }

    public void TrackWindow(Window window, string key)
    {
        window.Closed += (_, _) =>
        {
            ActiveWindows[key].Remove(window);

            if (!ActiveWindows[key].Any())
            {
                ActiveWindows.Remove(key);
            }
        };

        if (ActiveWindows.ContainsKey(key))
        {
            ActiveWindows[key].Add(window);
        }
        else
        {
            ActiveWindows.Add(key, new List<Window> { window });
        }
    }

    public Window GetWindowForElement(UIElement element, string key)
    {
        return element.XamlRoot == null
            ? null
            : ActiveWindows[key].FirstOrDefault(window => element.XamlRoot == window.Content.XamlRoot);
    }

    public Window GetWindowForElement(XamlRoot xamlRoot, string key)
    {
        return xamlRoot == null
            ? null
            : ActiveWindows[key].FirstOrDefault(window => xamlRoot == window.Content.XamlRoot);
    }
}