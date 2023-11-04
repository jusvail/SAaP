using SAaP.ViewModels;

namespace SAaP.Views;

public sealed partial class SettingsPage
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        this.InitializeComponent();

        InitViewModel();
    }

    private async void InitViewModel()
    {
        await ViewModel.ReadSettingsWhenInitialize();
    }
}