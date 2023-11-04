using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Core.Services.Generic;
using SAaP.Extensions;
using SAaP.Helper;
using SAaP.Views;

namespace SAaP.Services;

public class ActivationService : IActivationService
{
    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

#if MONITORPAGEONLY
        // navigate to main page
        App.NavigationRootPage.NavigateTo<MonitorPage>("MonitorPageTitle".GetLocalized(), null);

#endif

        // active main window
        App.MainWindow.Activate();
    }

    private static async Task InitializeAsync()
    {
        // folder tree creation
        await StartupService.EnsureWorkSpaceFolderTreeIntegrityAsync();

		Logger.EnsuredLogEnv();

		if (!DateSliderConverter.SliderRange.Any())
        {
	        // 上证指数
	        foreach (var originalData in await DbService.TakeOriginalDataFromFile("000001", StockService.ShFlag, 999))
		        DateSliderConverter.SliderRange.Add(DateTimeOffset.Parse(originalData.Day));
        }
    }
}