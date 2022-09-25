using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Core.Services;
using SAaP.Extensions;
using SAaP.Views;

namespace SAaP.Services;

public class ActivationService : IActivationService
{
    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();
        // navigate to main page
        App.MainWindow.NavigateTo<MainPage>(PjConstant.AppTitle.GetLocalized(), null);
        // active main window
        App.MainWindow.Activate();
    }

    private static async Task InitializeAsync()
    {
        // folder tree creation
        await StartupService.EnsureWorkSpaceFolderTreeIntegrityAsync();
    }
}