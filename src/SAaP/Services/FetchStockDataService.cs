using Windows.Storage;
using CommunityToolkit.WinUI.Helpers;
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

    public async Task FetchStockMinuteData(string pyArg, int minType)
    {
        var pyPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.PythonInstallationPath);

        if (string.IsNullOrEmpty(pyPath)) return;

        var tdxPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.TdxInstallationPath);

        if (string.IsNullOrEmpty(tdxPath)) return;

        var pyExecPath = Path.Combine(pyPath, PythonService.PyName);

        var pyScriptPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, PythonService.PyFolder, PythonService.TdxMinuteReader);

        var f = await StorageFile.GetFileFromPathAsync(pyScriptPath);

        if (f == null) return;

        // python script execution
        await PythonService.RunPythonScript(pyExecPath
            , pyScriptPath
            , tdxPath
            , StartupService.MinDataPath
            , minType.ToString()
            , pyArg);
    }

    public async Task<int> TryGetBelongByCode(string code)
    {
        var belongTo = -1;

        if (string.IsNullOrEmpty(code)) return belongTo;

        switch (code.Length)
        {
            case StockService.StandardCodeLength:
                {
                    var tdxPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.TdxInstallationPath);

                    var fileSh = StockService.GetInputNameSh(code);

                    if (tdxPath != null)
                    {
                        var folderSh = await StorageFolder.GetFolderFromPathAsync(tdxPath + StockService.ShPath);
                        var shExist = false;
                        var szExist = false;
                        if (await folderSh.FileExistsAsync(fileSh))
                        {
                            shExist = true;
                        }

                        var folderSz = await StorageFolder.GetFolderFromPathAsync(tdxPath + StockService.SzPath);

                        var fileSz = StockService.GetInputNameSz(code);

                        if (await folderSz.FileExistsAsync(fileSz))
                        {
                            szExist = true;
                        }

                        return shExist switch
                        {
                            true when szExist => StockService.MultiFlg,
                            false when !szExist => StockService.NotExistFlg,
                            true => StockService.ShFlag,
                            false => StockService.SzFlag
                        };
                    }

                    break;
                }
            case StockService.TdxCodeLength:
                var flg = code[..1];
                _ = int.TryParse(flg, out belongTo);
                break;
        }

        return belongTo;
    }
}