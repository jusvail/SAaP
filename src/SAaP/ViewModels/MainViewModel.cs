using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using SAaP.Core.Models.Analyst;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Analyst;
using SAaP.Core.Services.Generic;
using SAaP.Extensions;
using SAaP.Views;
using Windows.ApplicationModel.DataTransfer;

// using Microsoft.UI.Dispatching;

namespace SAaP.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
	// store csv output by py script to sqlite database
	private readonly IDbTransferService _dbTransferService;

	private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

	// fetch data
	private readonly IFetchStockDataService _fetchStockDataService;

	// restore settings service
	private readonly IRestoreSettingsService _restoreSettingsService;

	// analyze main service
	private readonly IStockAnalyzeService _stockAnalyzeService;

	// window Manager
	private readonly IWindowManageService _windowManageService;

	private bool   _analysisStarted;
	private string _codeInput;
	private string _currentStatus;
	private bool   _isQueryAllChecked;
	private string _lastingDays;
	private int    _selectedActivityDate;
	private int    _selectedFavGroupIndex;


	public MainViewModel(
		IDbTransferService dbTransferService
		, IStockAnalyzeService stockAnalyzeService
		, IRestoreSettingsService restoreSettingsService
		, IWindowManageService windowManageService
		, IFetchStockDataService fetchStockDataService)
	{
		_dbTransferService      = dbTransferService;
		_stockAnalyzeService    = stockAnalyzeService;
		_restoreSettingsService = restoreSettingsService;
		_windowManageService    = windowManageService;
		_fetchStockDataService  = fetchStockDataService;

		AddToQueryingCommand                = new RelayCommand<object>(AddToQuerying);
		ClearDataGridCommand                = new RelayCommand(OnClearDataGrid);
		MenuSettingsCommand                 = new RelayCommand(OnMenuSettingsPressed);
		NavigateToMonitorCommand            = new RelayCommand(NavigateToMonitorPage);
		NavigateToInvestLogCommand          = new RelayCommand(NavigateToInvestLogPage);
		AnalysisPressedCommand              = new AsyncRelayCommand(OnAnalysisPressedAsync);
		Analysis2PressedCommand             = new AsyncRelayCommand(OnAnalysis2PressedAsync);
		QueryHot100CodesCommand             = new AsyncRelayCommand(QueryHot100CodesAsync);
		DeleteSelectedFavoriteGroupsCommand = new AsyncRelayCommand<IList<object>>(DeleteSelectedFavoriteGroupsAsync);
		DeleteSelectedActivityCommand       = new AsyncRelayCommand<IList<object>>(DeleteSelectedActivityAsync);
		DeleteSelectedFavoriteCodesCommand  = new AsyncRelayCommand<object>(DeleteSelectedFavoriteCodesAsync);
		RedirectToAnalyzeDetailCommand      = new AsyncRelayCommand<object>(RedirectToAnalyzeDetailAsync);
	}

	public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

	public ObservableCollection<Report> AnalyzedResults2 { get; } = new();

	public ObservableCollection<string> FavoriteGroups { get; } = new();

	public ObservableCollection<string> ActivityDateList { get; } = new();

	public ObservableCollection<ActivityData> ActivityDates { get; } = new();

	public ObservableCollection<FavoriteDetail> GroupList { get; } = new();

	private IList<string> LastQueriedCodes { get; set; }

	public IRelayCommand ClearDataGridCommand { get; }

	public IRelayCommand MenuSettingsCommand { get; }

	public IRelayCommand NavigateToInvestLogCommand { get; }

	public IRelayCommand NavigateToMonitorCommand { get; }

	public IRelayCommand<object> AddToQueryingCommand { get; }

	public IAsyncRelayCommand AnalysisPressedCommand { get; }

	public IAsyncRelayCommand Analysis2PressedCommand { get; }

	public IAsyncRelayCommand QueryHot100CodesCommand { get; }

	public IAsyncRelayCommand<IList<object>> DeleteSelectedFavoriteGroupsCommand { get; }

	public IAsyncRelayCommand<IList<object>> DeleteSelectedActivityCommand { get; }

	public IAsyncRelayCommand<object> DeleteSelectedFavoriteCodesCommand { get; }

	public IAsyncRelayCommand<object> RedirectToAnalyzeDetailCommand { get; }

	public int SelectedFavGroupIndex
	{
		get => _selectedFavGroupIndex;
		set => SetProperty(ref _selectedFavGroupIndex, value);
	}

	public string CodeInput
	{
		get => _codeInput;
		set => SetProperty(ref _codeInput, value);
	}

	public string LastingDays
	{
		get => _lastingDays;
		set => SetProperty(ref _lastingDays, value);
	}

	public bool IsQueryAllChecked
	{
		get => _isQueryAllChecked;
		set => SetProperty(ref _isQueryAllChecked, value);
	}

	public int SelectedActivityDate
	{
		get => _selectedActivityDate;
		set => SetProperty(ref _selectedActivityDate, value);
	}

	public string CurrentStatus
	{
		get => _currentStatus;
		set => SetProperty(ref _currentStatus, value);
	}

	public bool AnalysisStarted
	{
		get => _analysisStarted;
		set => SetProperty(ref _analysisStarted, value);
	}

	private void NavigateToMonitorPage()
	{
		_windowManageService.CreateOrBackToWindow<MonitorPage>(typeof(MonitorViewModel).FullName!);
	}

	private void NavigateToInvestLogPage()
	{
		_windowManageService.CreateOrBackToWindow<InvestLogPage>(typeof(InvestLogViewModel).FullName!);
	}

	private async Task QueryHot100CodesAsync()
	{
		var codes = StockService.PostHot100Codes();

		var codeFormat = new StringBuilder();

		await foreach (var code in codes)
			if (!string.IsNullOrEmpty(code))
				codeFormat.Append(StockService.ReplaceLocStringToFlag(code.ToLower())).Append(',');

		CodeInput = codeFormat.ToString();
	}

	private async Task RedirectToAnalyzeDetailAsync(object obj)
	{
		var codeName = obj as string;

		if (string.IsNullOrEmpty(codeName)) return;

		codeName = StockService.ReplaceLocStringToFlag(codeName);

		var companyName = await StockService.FetchCompanyNameByCode(codeName).ConfigureAwait(true);

		var title = "AnalyzeDetailPageTitle".GetLocalized() + $": [{codeName} {companyName}]";

		_windowManageService.CreateOrBackToWindow<AnalyzeDetailPage>(typeof(AnalyzeDetailViewModel).FullName!, title,
		                                                             codeName);
	}

	private void OnMenuSettingsPressed()
	{
		_windowManageService.CreateOrBackToWindow<SettingsPage>(typeof(SettingsViewModel).FullName!);
	}

	public void AddToQuerying(object listView)
	{
		var lv = listView as ListView;

		if (lv == null) return;

		var accuracyCodes = StringHelper.FormatInputCode(CodeInput) ?? new List<string>();

		for (var i = 0; i < lv.SelectedItems.Count; i++)
		{
			var select = (FavoriteDetail)lv.SelectedItems[i];
			if (!accuracyCodes.Contains(select.CodeName)) accuracyCodes.Add(select.CodeName);
		}

		CodeInput = StockService.FormatPyArgument(accuracyCodes);
	}

	private async Task DeleteSelectedFavoriteCodesAsync(object listView)
	{
		var lv = listView as ListView;

		if (lv == null) return;

		var selectedList = lv.SelectedItems;

		for (var i = 0; i < selectedList.Count; i++)
		{
			var favorite = (FavoriteDetail)selectedList[i];
			await _dbTransferService.DeleteFavoriteCodes(new FavoriteData
			{
				Id        = favorite.GroupId,
				GroupName = favorite.GroupName,
				BelongTo  = Convert.ToInt32(favorite.CodeName[..1]),
				Code      = favorite.CodeName[1..]
			});
		}

		// reload group
		await RefreshFavoriteGroupAsync(FavoriteGroups[SelectedFavGroupIndex]);
	}

	private async Task DeleteSelectedFavoriteGroupsAsync(IList<object> selectedItems)
	{
		if (!selectedItems.Any()) return;

		for (var i = 0; i < selectedItems.Count; i++)
		{
			var group = (string)selectedItems[i];
			await _dbTransferService.DeleteFavoriteGroups(group);
		}

		// restore FavoriteGroups is necessary
		await BackupCurrentSelectGroupAndRestoreFavoriteGroupsAsync();
	}

	private async Task DeleteSelectedActivityAsync(IList<object> selectedItems)
	{
		if (!selectedItems.Any()) return;

		for (var i = 0; i < selectedItems.Count; i++)
		{
			var group = (string)selectedItems[i];
			await _dbTransferService.DeleteActivity(group);
		}

		// restore Activity is necessary
		await BackupCurrentSelectActivityListAndRestoreSelectedAsync();
	}

	private async Task BackupCurrentSelectGroupAndRestoreFavoriteGroupsAsync()
	{
		var currentGroup = string.Empty;

		if (FavoriteGroups.Any()) currentGroup = FavoriteGroups[SelectedFavGroupIndex];

		// restore favorite group comboBox
		await RestoreFavoriteGroupsAsync();

		if (FavoriteGroups.Any())
		{
			// will trigger FavoriteListSelectionChanged
			var index = FavoriteGroups.IndexOf(currentGroup);
			// user may delete a group displayed currently
			SelectedFavGroupIndex = index > 0 ? index : 0;
		}
	}

	public async Task AddToFavoriteAsync(string groupName, string codes)
	{
		var accuracyCodes = StringHelper.FormatInputCode(codes);

		if (!accuracyCodes.Any()) return;

		foreach (var accuracyCode in accuracyCodes) await _dbTransferService.AddToFavorite(accuracyCode, groupName);

		await BackupCurrentSelectGroupAndRestoreFavoriteGroupsAsync();
	}

	private void OnClearDataGrid()
	{
		AnalyzedResults.Clear();
	}

	// private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

	// private void SetValueCrossThread(Action action)
	// {
	// 	_dispatcherQueue.TryEnqueue(() => { action(); });
	// }

	private async Task OnAnalysis2PressedAsync()
	{
		var startTime = DateTime.Now;
		//store last queried codes
		LastQueriedCodes = StringHelper.FormatInputCode(CodeInput);

		if (LastQueriedCodes == null || !LastQueriedCodes.Any()) return;

		SetCurrentStatus("保存历史查询数据。。。");
		AnalysisStarted = true;

		// store this activity
		await Task.Run(async () => { await _dbTransferService.StoreActivityDataToDb(DateTime.Now, CodeInput, string.Empty); });

		AnalyzedResults2.Clear();

		var reports = new List<Report>();

		SetCurrentStatus("开始分析数据。。。");

		// analyze start
		var all = LastQueriedCodes.Count;
		for (var i = 0; i < LastQueriedCodes.Count; i++)
			try
			{
				var codeName              = LastQueriedCodes[i];
				var fetchStockDataService = App.GetService<IFetchStockDataService>();
				var belong                = await fetchStockDataService.TryGetBelongByCode(codeName);

				var codeMain    = StockService.CutStockCodeToSix(codeName);
				var companyName = await StockService.FetchCompanyNameByCode(codeMain, belong);

				SetCurrentStatus($"正在分析：{companyName} 进度：{i + 1}/{all}");

				// query original data recently
				var originalData = await DbService.TakeOriginalDataAscending(codeMain, belong);

				if (!originalData.Any()) continue;

				var raw = new RawData(new Stock { CodeName = codeMain, CompanyName = companyName, BelongTo = belong }, originalData);

				await Task.Run(() =>
				{
					var analyst = new Analyst(raw);

					reports.Add(analyst.Target());
					// SetValueCrossThread(()=> SetCurrentStatus($"正在分析({index}/{all})"));
				});
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Console.WriteLine(GetType());
			}

		foreach (var report in reports) AnalyzedResults2.Add(report);

		await Task.Delay(1000);
		var endTime = DateTime.Now;
		AnalysisStarted = false;
		SetCurrentStatus("用时：" + (endTime - startTime).Seconds + "秒");
	}

	private async Task OnAnalysisPressedAsync()
	{
		var startTime = DateTime.Now;
		SetCurrentStatus("开始执行。。。");

		// check code accuracy
		var accuracyCodes = StringHelper.FormatInputCode(CodeInput);
		// check null input
		if (accuracyCodes == null) return;

		AnalysisStarted = true;

		// add comma
		var pyArg = StockService.FormatPyArgument(accuracyCodes);

		SetCurrentStatus("检查输入。。。");

		// formatted code resetting
		CodeInput = pyArg;

		if (IsQueryAllChecked)
		{
			SetCurrentStatus("开始执行py脚本。。。");

			// fetch stock data from tdx, then store to csv file
			await _fetchStockDataService.FetchStockData(pyArg, IsQueryAllChecked);

			SetCurrentStatus("py脚本执行完毕，开始将数据导入至本地数据库");
		}

		try
		{
			// transfer stock data to sqlite database
			await Task.Run(async () => { await _dbTransferService.TransferCsvDataToDbAsync(accuracyCodes, IsQueryAllChecked); });
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			Console.WriteLine(GetType());
		}

		//store last queried  codes
		LastQueriedCodes = accuracyCodes;

		SetCurrentStatus("保存历史查询数据。。。");

		// store this activity
		await Task.Run(async () => { await _dbTransferService.StoreActivityDataToDb(DateTime.Now, pyArg, string.Empty); });

		SetCurrentStatus("开始分析数据。。。");

		// invoke analyze
		await OnLastingDaysValueChangedAsync();

		SetCurrentStatus("完成。。。");

		// query history update
		await BackupCurrentSelectActivityListAndRestoreSelectedAsync();
		// bring window back
		_windowManageService.SetWindowForeground(App.MainWindow);

		await Task.Delay(1000);
		var endTime = DateTime.Now;
		AnalysisStarted = false;
		SetCurrentStatus("用时：" + (endTime - startTime).Seconds + "秒");
	}

	public async Task OnLastingDaysValueChangedAsync()
	{
		// if none int
		if (!int.TryParse(LastingDays, out var duration)) return;

		// ignore less than 5 days analyze
		if (duration > 5)
		{
			// clear preview result
			AnalyzedResults.Clear();

			var res = new ObservableCollection<AnalysisResult>();

			if (LastQueriedCodes == null) return;

			var allCount = LastQueriedCodes.Count;
			// analyze start
			for (var i = 0; i < LastQueriedCodes.Count; i++)
			{
				var code = LastQueriedCodes[i];
				var data = await _stockAnalyzeService.AnalyzeAsync(code, duration);
				if (data == null) continue;
				SetCurrentStatus($"正在分析：{data.CompanyName} 进度：{i + 1}/{allCount}");
				res.Add(data);
				// SetCurrentStatus($"正在分析({++index}/{all})");
			}

			foreach (var result in res) AnalyzedResults.Add(result);
		}
	}

	public async Task RestoreFavoriteGroupsAsync()
	{
		var groups = (await _restoreSettingsService.GetFavoriteGroupsName()).ToList();

		FavoriteGroups.Clear();

		if (!groups.Any())
			FavoriteGroups.Add("自选股");
		else
			foreach (var group in groups)
				FavoriteGroups.Add(group);
	}

	private async Task RefreshFavoriteGroupAsync(string selectedGroup)
	{
		var favorites = _restoreSettingsService.RestoreFavoriteCodesString(selectedGroup);

		GroupList.Clear();

		await foreach (var favorite in favorites) GroupList.Add(favorite);
	}

	public async Task RestoreLastQueryStringAsync()
	{
		// get service
		var restoreSettingsService = App.GetService<IRestoreSettingsService>();
		// query from db
		var lastQuery = await restoreSettingsService.RestoreLastQueryStringFromDb();

		if (lastQuery != null) CodeInput = lastQuery;
	}

	public async Task RestoreActivityAsync()
	{
		// get all activity date
		var activityDates = _restoreSettingsService.RestoreRecentlyActivityGroupByDate();
		// clear first is needed
		ActivityDateList.Clear();

		await foreach (var activityDate in activityDates)
			// add to list
			ActivityDateList.Add(activityDate);

		if (!ActivityDateList.Any())
			// if no data, add today
			ActivityDateList.Add(DateTime.Today.ToString(PjConstant.DateFormatUsedToCompare));
	}

	private async Task RefreshActivityListAsync(string date)
	{
		if (SelectedActivityDate == -1) return;

		var activityDates = _restoreSettingsService.RestoreRecentlyActivityListByDate(date);

		//clear first
		ActivityDates.Clear();

		await foreach (var activityDate in activityDates)
			// add to list
			ActivityDates.Add(activityDate);
	}

	private async Task BackupCurrentSelectActivityListAndRestoreSelectedAsync()
	{
		var currentActivityDate = string.Empty;

		if (ActivityDateList.Any()) currentActivityDate = ActivityDateList[SelectedActivityDate];

		// restore favorite group comboBox
		await RestoreActivityAsync();

		if (ActivityDateList.Any())
		{
			// will trigger FavoriteListSelectionChanged
			var index = ActivityDateList.IndexOf(currentActivityDate);
			// user may delete a group displayed currently
			SelectedActivityDate = index > 0 ? index : 0;
		}
	}

	public async void FavoriteListSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (SelectedFavGroupIndex == -1) return;
		await RefreshFavoriteGroupAsync(FavoriteGroups[SelectedFavGroupIndex]);
	}

	public async void QueryHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (SelectedActivityDate == -1) return;
		await RefreshActivityListAsync(ActivityDateList[SelectedActivityDate]);
	}

	public void ActivityDateClicked(object sender, ItemClickEventArgs e)
	{
		if (e.ClickedItem is not ActivityData activityData) return;

		CodeInput = activityData.QueryString;
	}

	private void SetValueCrossThread(Action action)
	{
		_dispatcherQueue.TryEnqueue(() => { action(); });
	}

	private void SetCurrentStatus(string status)
	{
		SetValueCrossThread(() => { CurrentStatus = status; });
	}

	public async Task FormatCodeInput(string input)
	{
		var list = _fetchStockDataService.FormatInputCodeAsync(input);
		CodeInput = await StockService.FormatPyArgumentAsync(list);
	}

	//[RelayCommand]
	//public void DataGridMenuFlyoutItemCopy(object gridView)
	//{
	//	var gridview = gridView as GridView;


	//	var dataPackage = new DataPackage
	//	{
	//		RequestedOperation = DataPackageOperation.Copy
	//	};

	//	var sb = new StringBuilder();

	//	try
	//	{
	//		var button = (DataGrid)gridView;

	//		for (var index = 0; index < button.SelectedItems.Count; index++)
	//		{
	//			var stock = (Report)button.SelectedItems[index];
	//			sb.Append(stock.CodeName[1..]).Append(" ");
	//		}

	//		dataPackage.SetText(sb.ToString());
	//		Clipboard.SetContent(dataPackage);
	//	}
	//	catch (Exception)
	//	{
	//		Console.WriteLine();
	//	}

	//}

}