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
            ActiveWindows.Remove(window);
        };

        window.Activate();

        ActiveWindows.Add(window);

        _activeWindowsKeys.Add(key);
    }


    public Window GetWindowForElement(UIElement element)
    {
        return element.XamlRoot == null
            ? null
            : ActiveWindows.FirstOrDefault(window => element.XamlRoot == window.Content.XamlRoot);
    }
}