using Windows.Storage;
using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Core.Services;

namespace SAaP.Services;

public class FetchStockDataService : IFetchStockDataService
{

    // settings service
    private readonly ILocalSettingsService _localSettingsService;

    public FetchStockDataService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task FetchStockData(string pyArg, bool isCheckAll = false)
    {
        var pyPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.PythonInstallationPath);

        if (string.IsNullOrEmpty(pyPath)) return;

        var tdxPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.TdxInstallationPath);

        if (string.IsNullOrEmpty(tdxPath)) return;

        var pyExecPath = Path.Combine(pyPath, PythonService.PyName);

        //const string pyScriptPath = "C:\\Workspace\\WK\\blk test\\tdx_reader.py";
        var pyScriptPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, PythonService.PyFolder, PythonService.TdxReader);

        var f = await StorageFile.GetFileFromPathAsync(pyScriptPath);

        if (f == null) return;

        // python script execution
        await PythonService.RunPythonScript(pyExecPath
            , pyScriptPath
            , tdxPath
            , StartupService.PyDataPath
            , isCheckAll ? string.Empty : pyArg);
    }
}