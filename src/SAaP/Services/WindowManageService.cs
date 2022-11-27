using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Enum;
using SAaP.Contracts.Services;
using SAaP.Extensions;
using SAaP.Helper;
using WinRT.Interop;

namespace SAaP.Services;

public class WindowManageService : IWindowManageService
{
    private readonly IPageService _pageService;

    public static Dictionary<string, List<Window>> ActiveWindows { get; } = new();

    public WindowManageService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public void SetWindowForeground(Window window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);

        //RuntimeHelper.ShowWindow(hwnd, 9); // 9 => SW_RESTORE：激活并显示窗口
        RuntimeHelper.BringWindowToTop(hwnd);
    }

    public MainWindow CreateWindow(string key)
    {
        var newWindow = new MainWindow();
        TrackWindow(newWindow, key);

        return newWindow;
    }

    public void CreateOrBackToWindow<T>(string key, string title = null, object arg = null) where T : Page
    {
        if (!string.IsNullOrEmpty(key))
        {
            lock (ActiveWindows)
            {
                if (ActiveWindows.Keys.Contains(key))
                {
                    if (InstanceType.Single == _pageService.GetPageInstanceType(key))
                    {
                        SetWindowForeground(ActiveWindows[key].First());
                        return;
                    }

                    if (InstanceType.Multi == _pageService.GetPageInstanceType(key))
                    {
                        foreach (var activatedWindow in ActiveWindows[key].Where(activatedWindow => title != null && activatedWindow.Title.Contains(title)))
                        {
                            SetWindowForeground(activatedWindow);
                            return;
                        }
                    }
                }
            }
        }

        var window = CreateWindow(key);

        var windowTitle = string.IsNullOrEmpty(title) ? $"{typeof(T).Name}Title".GetLocalized() : title;

        window.Title = windowTitle;
        window.NavigateTo<T>(windowTitle, arg);

        window.Activate();

        var context = new DispatcherQueueSynchronizationContext(window.DispatcherQueue);

        SynchronizationContext.SetSynchronizationContext(context);
    }

    public void TrackWindow(Window window, string key)
    {
        window.Closed += (_, _) =>
        {
            if (!ActiveWindows.Any()) return;
            if (!ActiveWindows.ContainsKey(key)) return;

            if (ActiveWindows[key].Contains(window))
            {
                ActiveWindows[key].Remove(window);
            }

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
        return GetWindowForElement(element.XamlRoot, key);
    }

    public Window GetWindowForElement(XamlRoot xamlRoot, string key)
    {
        return xamlRoot == null
            ? null
            : ActiveWindows[key].FirstOrDefault(window => xamlRoot == window.Content.XamlRoot);
    }
}