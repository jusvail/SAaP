#nullable enable
using Microsoft.UI.Xaml;

namespace SAaP.Contracts.Services;

public interface IWindowManageService
{
    void CreateWindowAndNavigateTo(string key);

    Window CreateWindow();

    void TrackWindow(Window window);

    Window GetWindowForElement(UIElement element);

    Window GetWindowForElement(XamlRoot xamlRoot);
}