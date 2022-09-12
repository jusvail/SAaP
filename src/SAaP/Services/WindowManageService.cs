using Microsoft.UI.Xaml;
using SAaP.Contracts.Enum;
using SAaP.Contracts.Services;
using SAaP.Extensions;

namespace SAaP.Services;

public class WindowManageService : IWindowManageService
{
    private readonly IPageService _pageService;

    private readonly List<string> _activeWindowsKeys = new();

    public List<Window> ActiveWindows { get; } = new();

    public WindowManageService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public Window CreateWindow()
    {
        var newWindow = new Window();
        TrackWindow(newWindow);
        return newWindow;
    }

    public void CreateWindowAndNavigateTo(string key)
    {
        lock (_activeWindowsKeys)
        {
            if (_activeWindowsKeys.Contains(key)
                && InstanceType.Single == _pageService.GetPageInstanceType(key))
            {
                return;
            }
        }

        var type = _pageService.GetPageType(key);

        var uiElement = Activator.CreateInstance(type) as UIElement;

        if (uiElement == null) return;
        var window = new Window
        {
            Content = uiElement,
            Title = $"{type.Name}Title".GetLocalized()
        };

        window.Closed += (_, _) =>
        {
            _activeWindowsKeys.Remove(key);
        };

        TrackWindow(window);

        window.Activate();

        _activeWindowsKeys.Add(key);
    }
    public void TrackWindow(Window window)
    {
        window.Closed += (_, _) =>
        {
            ActiveWindows.Remove(window);
        };
        ActiveWindows.Add(window);
    }

    public Window GetWindowForElement(UIElement element)
    {
        return element.XamlRoot == null
            ? null
            : ActiveWindows.FirstOrDefault(window => element.XamlRoot == window.Content.XamlRoot);
    }

    public Window GetWindowForElement(XamlRoot xamlRoot)
    {
        return xamlRoot == null
            ? null
            : ActiveWindows.FirstOrDefault(window => xamlRoot == window.Content.XamlRoot);
    }
}