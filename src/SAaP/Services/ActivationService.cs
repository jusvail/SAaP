using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Core.Services;
using SAaP.Extensions;
using SAaP.Views;
using static SAaP.MainWindow;

namespace SAaP.Services;

public class ActivationService : IActivationService
{
    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        App.MainWindow.NavigateTo<MainPage>(PjConstant.AppTitle.GetLocalized(), null);

        App.MainWindow.Activate();
    }

    private static async Task InitializeAsync()
    {
        // folder tree creation
        await StartupService.EnsureWorkSpaceFolderTreeIntegrityAsync();
    }
}