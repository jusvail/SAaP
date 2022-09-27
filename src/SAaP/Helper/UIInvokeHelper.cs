using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace SAaP.Helper;

internal class UiInvokeHelper
{
    public static void Invoke(Button button)
    {
        var peer = new ButtonAutomationPeer(button);
        var provider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

        provider?.Invoke();
    }

    public static void HideButtonFlyOut(Button button)
    {
        if (button == null) return;
        button.Flyout.Hide();
    }
}