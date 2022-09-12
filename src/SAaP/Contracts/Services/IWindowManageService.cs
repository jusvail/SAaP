using Microsoft.UI.Xaml;

namespace SAaP.Contracts.Services;

public interface IWindowManageService
{
    void CreateWindowAndNavigateTo(string key);

    Window GetWindowForElement(UIElement element);
}