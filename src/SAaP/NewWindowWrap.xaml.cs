using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SAaP.Helper;
using Windows.ApplicationModel;
using Microsoft.UI.Windowing;
using WinRT;
using WinRT.Interop;

namespace SAaP;

internal class WindowsSystemDispatcherQueueHelper
{
	[StructLayout(LayoutKind.Sequential)]
	private struct DispatcherQueueOptions
	{
		internal int dwSize;
		internal int threadType;
		internal int apartmentType;
	}

	[DllImport("CoreMessaging.dll")]
	private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

	object _dispatcherQueueController;

	public void EnsureWindowsSystemDispatcherQueueController()
	{
		if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
		{
			// one already exists, so we'll just use it.
			return;
		}

		if (_dispatcherQueueController != null) return;

		DispatcherQueueOptions options;
		options.dwSize        = Marshal.SizeOf(typeof(DispatcherQueueOptions));
		options.threadType    = 2; // DQTYPE_THREAD_CURRENT
		options.apartmentType = 2; // DQTAT_COM_STA

		CreateDispatcherQueueController(options, ref _dispatcherQueueController);
	}
}
public sealed partial class NewWindowWrap
{
    public NewWindowWrap()
    {
        InitializeComponent();

        ((FrameworkElement)Content).RequestedTheme = ThemeHelper.RootTheme;

        var wsdqHelper = new WindowsSystemDispatcherQueueHelper();
        wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

        SetBackdrop(BackdropType.Mica);
    }

    public void NavigateTo<T>(string title, object arg) where T : Page
    {
        ExtendsContentIntoTitleBar = true;
        AppTitleBar.Visibility = Visibility.Visible;

        SetTitleBar(AppTitleBar);
        AppTitleBarText.Text = title;

        SetBackdrop(BackdropType.Mica);

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/BadgeLogo.ico"));

        // frame navigate to
        NavigationFrame.Navigate(typeof(T), arg, new DrillInNavigationTransitionInfo());
    }

    public enum BackdropType
    {
        Mica,
        DesktopAcrylic,
        DefaultColor,
    }

    public BackdropType CurrentBackdrop { get; private set; }

    Microsoft.UI.Composition.SystemBackdrops.MicaController _micaController;
    Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController _acrylicController;
    Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration _configurationSource;

    public void SetBackdrop(BackdropType type)
    {
        // Reset to default color. If the requested type is supported, we'll update to that.
        // Note: This sample completely removes any previous controller to reset to the default
        //       state. This is done so this sample can show what is expected to be the most
        //       common pattern of an app simply choosing one controller type which it sets at
        //       startup. If an app wants to toggle between Mica and Acrylic it could simply
        //       call RemoveSystemBackdropTarget() on the old controller and then setup the new
        //       controller, reusing any existing _configurationSource and Activated/Closed
        //       event handlers.
        CurrentBackdrop = BackdropType.DefaultColor;

        if (_micaController != null)
        {
            _micaController.Dispose();
            _micaController = null;
        }
        if (_acrylicController != null)
        {
            _acrylicController.Dispose();
            _acrylicController = null;
        }
        this.Activated -= Window_Activated;
        this.Closed -= Window_Closed;
        ((FrameworkElement)this.Content).ActualThemeChanged -= Window_ThemeChanged;
        _configurationSource = null;

        if (type == BackdropType.Mica)
        {
            if (TrySetMicaBackdrop())
            {
                CurrentBackdrop = type;
            }
            else
            {
                // Mica isn't supported. Try Acrylic.
                type = BackdropType.DesktopAcrylic;
            }
        }
        if (type == BackdropType.DesktopAcrylic)
        {
            if (TrySetAcrylicBackdrop())
            {
                CurrentBackdrop = type;
            }
        }
    }

    bool TrySetMicaBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            // Hooking up the policy object
            _configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;

            // Initial configuration state.
            _configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();
            ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

            _micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

            //_micaController.Kind = MicaKind.BaseAlt;

            try
            {
	            // Enable the system backdrop.
	            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
	            _micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
	            _micaController.SetSystemBackdropConfiguration(_configurationSource);
	            return true; // succeeded
            }
            catch (Exception e)
            {
	            Console.WriteLine(e);
	            return false;
            }
        }

        return false; // Mica is not supported on this system
    }

    bool TrySetAcrylicBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            // Hooking up the policy object
            _configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
            this.Activated += Window_Activated;
            this.Closed += Window_Closed;
            ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            _configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            _acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
            return true; // succeeded
        }

        return false; // Acrylic is not supported on this system
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (_micaController != null)
        {
            _micaController.Dispose();
            _micaController = null;
        }
        if (_acrylicController != null)
        {
            _acrylicController.Dispose();
            _acrylicController = null;
        }
        this.Activated -= Window_Activated;
        _configurationSource = null;
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    private void SetConfigurationSourceTheme()
    {
        if (Content == null) return;

        switch (((FrameworkElement)this.Content).ActualTheme)
        {
            case ElementTheme.Dark: _configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: _configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
            case ElementTheme.Default: _configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
        }
    }
}