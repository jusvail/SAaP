#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SAaP.Contracts.Services;

public interface IWindowManageService
{
    void CreateOrBackToWindow<T>(string key, string? title = null, object? arg = null) where T : Page;

    NewWindowWrap CreateWindow(string key);

    void SetWindowForeground(Window window);

    void TrackWindow(Window window, string key);

}