#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SAaP.Contracts.Services;

public interface IWindowManageService
{
    void CreateWindowAndNavigateTo<T>(string key, string title, object arg) where T : Page;

    Window CreateWindow(string key );

    void TrackWindow(Window window, string key);

    Window GetWindowForElement(UIElement element, string key);

    Window GetWindowForElement(XamlRoot xamlRoot, string key);
}