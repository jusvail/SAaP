using Windows.Storage;
using CommunityToolkit.WinUI.Helpers;
using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Core.Helpers;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Services;

public class FetchStockDataService : IFetchStockDataService
{
	// settings service
	private readonly ILocalSettingsService _localSettingsService;

	private string _pyPath;
	private string _tdxPath;

	public FetchStockDataService(ILocalSettingsService localSettingsService)
	{
		_localSettingsService = localSettingsService;

		Init();
	}


	public async Task FetchStockData(string pyArg, bool isCheckAll = false)
	{
		if (string.IsNullOrEmpty(_pyPath) || string.IsNullOrEmpty(_tdxPath)) return;

		var pyExecPath = Path.Combine(_pyPath, PythonService.PyName);

		//const string pyScriptPath = "C:\\Workspace\\WK\\blk test\\tdx_reader.py";
		var pyScriptPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, PythonService.PyFolder, PythonService.TdxReader);

		if (await StorageFile.GetFileFromPathAsync(pyScriptPath) == null) return;

		// python script execution
		await PythonService.RunPythonScript(pyExecPath
											, pyScriptPath
											, _tdxPath
											, StartupService.PyDataPath
											, isCheckAll ? string.Empty : pyArg);
	}

	public async Task FetchStockMinuteData(string pyArg, int minType)
	{
		//var pyPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.PythonInstallationPath);

		if (string.IsNullOrEmpty(_pyPath)) return;

		//var tdxPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.TdxInstallationPath);

		if (string.IsNullOrEmpty(_tdxPath)) return;

		var pyExecPath = Path.Combine(_pyPath, PythonService.PyName);

		var pyScriptPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, PythonService.PyFolder,
										PythonService.TdxMinuteReader);

		var f = await StorageFile.GetFileFromPathAsync(pyScriptPath);

		if (f == null) return;

		// python script execution
		await PythonService.RunPythonScript(pyExecPath
											, pyScriptPath
											, _tdxPath
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
					//var tdxPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.TdxInstallationPath);

					var fileSh = StockService.GetInputNameSh(code);

					if (_tdxPath != null)
					{
						var folderSh = await StorageFolder.GetFolderFromPathAsync(_tdxPath + StockService.ShPath);
						var shExist = false;
						var szExist = false;

						if (await folderSh.FileExistsAsync(fileSh)) shExist = true;

						var folderSz = await StorageFolder.GetFolderFromPathAsync(_tdxPath + StockService.SzPath);

						var fileSz = StockService.GetInputNameSz(code);

						if (await folderSz.FileExistsAsync(fileSz)) szExist = true;

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
			default:
				belongTo = StockService.UsFlag;
				break;
		}

		return belongTo;
	}

	public async Task<List<string>> FormatInputCode(string input)
	{
		// check code accuracy
		var accuracyCodes = StringHelper.FormatInputCode(input);
		// check null input
		if (accuracyCodes == null) return null;

		var allCodes = new List<string>();

		var inputCodeAsync = FormatInputCodeAsync(input);

		await foreach (var code in inputCodeAsync) allCodes.Add(code);

		// add comma 
		return allCodes;
	}

	public async IAsyncEnumerable<string> FormatInputCodeAsync(string input)
	{
		// check code accuracy
		var accuracyCodes = StringHelper.FormatInputCode(input);
		// check null input
		if (accuracyCodes == null) yield break;

		foreach (var accuracyCode in accuracyCodes)
		{
			if (accuracyCode.Length == StockService.TdxCodeLength) yield return accuracyCode;
			if (accuracyCode.Length == StockService.StandardCodeLength)
			{
				var belong = await TryGetBelongByCode(accuracyCode);
				switch (belong)
				{
					case StockService.MultiFlg:
						yield return StockService.ShFlag + accuracyCode;
						yield return StockService.SzFlag + accuracyCode;
						break;
					case StockService.ShFlag:
					case StockService.SzFlag:
						yield return belong + accuracyCode;
						break;
					case StockService.NotExistFlg:
						break;
				}
			}
			else
			{
				if (int.TryParse(accuracyCode, out _))
				{
					yield break;
				}

				// non CN stock
				yield return accuracyCode;
			}
		}
	}

	public async Task<Stock> GenerateStock(string codeName)
	{
		var belong = await TryGetBelongByCode(codeName);
		var codeMain = StockService.CutStockCodeLen7ToLen6(codeName);
		var companyName = await StockService.FetchCompanyNameByCode(codeMain, belong);

		return new Stock { CodeName = codeMain, CompanyName = companyName, BelongTo = belong };
	}

	public async IAsyncEnumerable<Stock> GenerateStocks(IList<string> codeNames)
	{
		foreach (var codeName in codeNames)
		{
			var belong = await TryGetBelongByCode(codeName);
			var codeMain = StockService.CutStockCodeLen7ToLen6(codeName);
			var companyName = await StockService.FetchCompanyNameByCode(codeMain, belong);

			yield return new Stock { CodeName = codeMain, CompanyName = companyName, BelongTo = belong };
		}
	}

	private async void Init()
	{
		_pyPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.PythonInstallationPath);
		_tdxPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.TdxInstallationPath);
	}
}