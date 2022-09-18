using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using SAaP.Contracts.Services;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;
using SAaP.Constant;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Storage;
using SAaP.Extensions;
using SAaP.Views;

namespace SAaP.ViewModels;

public class MainViewModel : ObservableRecipient
{
    // store csv output by py script to sqlite database
    private readonly IDbTransferService _dbTransferService;
    // analyze main service
    private readonly IStockAnalyzeService _stockAnalyzeService;
    // restore settings service
    private readonly IRestoreSettingsService _restoreSettingsService;
    // window Manager
    private readonly IWindowManageService _windowManageService;
    // settings service
    private readonly ILocalSettingsService _localSettingsService;

    private bool _isQueryAllChecked;
    private int _selectedFavGroupIndex;
    private int _selectedActivityDate;
    private string _codeInput;
    private string _lastingDays;
    private string _currentStatus;

    public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

    public ObservableCollection<string> FavoriteGroups { get; } = new();

    public ObservableCollection<string> ActivityDateList { get; } = new();

    public ObservableCollection<ActivityData> ActivityDates { get; } = new();

    public ObservableCollection<FavoriteDetail> GroupList { get; } = new();

    private IList<string> LastQueriedCodes { get; set; }

    public IRelayCommand ClearDataGridCommand { get; }

    public IRelayCommand MenuSettingsCommand { get; }

    public IRelayCommand<object> AddToQueryingCommand { get; }

    public IAsyncRelayCommand AnalysisPressedCommand { get; }

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

    public MainViewModel(
        IDbTransferService dbTransferService
        , IStockAnalyzeService stockAnalyzeService
        , IRestoreSettingsService restoreSettingsService
        , IWindowManageService windowManageService
        , ILocalSettingsService localSettingsService)
    {
        _dbTransferService = dbTransferService;
        _stockAnalyzeService = stockAnalyzeService;
        _restoreSettingsService = restoreSettingsService;
        _windowManageService = windowManageService;
        _localSettingsService = localSettingsService;

        AnalysisPressedCommand = new AsyncRelayCommand(OnAnalysisPressed);
        ClearDataGridCommand = new RelayCommand(OnClearDataGrid);
        DeleteSelectedFavoriteGroupsCommand = new AsyncRelayCommand<IList<object>>(DeleteSelectedFavoriteGroups);
        DeleteSelectedFavoriteCodesCommand = new AsyncRelayCommand<object>(DeleteSelectedFavoriteCodes);
        DeleteSelectedActivityCommand = new AsyncRelayCommand<IList<object>>(DeleteSelectedActivity);
        AddToQueryingCommand = new RelayCommand<object>(AddToQuerying);
        MenuSettingsCommand = new RelayCommand(OnMenuSettingsPressed);
        RedirectToAnalyzeDetailCommand = new AsyncRelayCommand<object>(RedirectToAnalyzeDetail);
        QueryHot100CodesCommand = new AsyncRelayCommand(QueryHot100Codes);
    }

    private async Task QueryHot100Codes()
    {
        var codes = StockService.PostHot100Codes();

        var codeFormat = new StringBuilder();

        await foreach (var code in codes)
        {
            if (!string.IsNullOrEmpty(code) && code.Length == 8)
            {
                codeFormat.Append(code[2..]).Append(",");
            }
        }

        CodeInput = codeFormat.ToString();
    }

    private async Task RedirectToAnalyzeDetail(object obj)
    {
        var codeName = obj as string;

        if (string.IsNullOrEmpty(codeName)) return;

        var companyName = await StockService.FetchCompanyNameByCode(codeName);

        var title = "AnalyzeDetailPageTitle".GetLocalized() + $": [{codeName} {companyName}]";

        _windowManageService.CreateWindowAndNavigateTo<AnalyzeDetailPage>(typeof(AnalyzeDetailViewModel).FullName!, title, obj);
    }

    private void OnMenuSettingsPressed()
    {
        _windowManageService.CreateWindowAndNavigateTo<SettingsPage>(typeof(SettingsViewModel).FullName!, null!, null!);
    }

    public void AddToQuerying(object listView)
    {
        var lv = listView as ListView;

        if (lv == null) return;

        var accuracyCodes = StringHelper.FormatInputCode(CodeInput) ?? new List<string>();

        foreach (FavoriteDetail select in lv.SelectedItems)
        {
            if (!accuracyCodes.Contains(select.CodeName)) accuracyCodes.Add(select.CodeName);
        }

        CodeInput = StockService.FormatPyArgument(accuracyCodes);
    }

    private async Task DeleteSelectedFavoriteCodes(object listView)
    {
        var lv = listView as ListView;

        if (lv == null) return;

        var selectedList = lv.SelectedItems;

        for (var i = 0; i < selectedList.Count; i++)
        {
            var favorite = (FavoriteDetail)selectedList[i];
            await _dbTransferService.DeleteFavoriteCodes(new FavoriteData
            {
                Id = favorite.GroupId,
                GroupName = favorite.GroupName,
                Code = favorite.CodeName
            });
        }
        // reload group
        await RefreshFavoriteGroup(FavoriteGroups[SelectedFavGroupIndex]);
    }

    private async Task DeleteSelectedFavoriteGroups(IList<object> selectedItems)
    {
        if (!selectedItems.Any()) return;

        foreach (string group in selectedItems)
        {
            await _dbTransferService.DeleteFavoriteGroups(group);
        }

        // restore FavoriteGroups is necessary
        await BackupCurrentSelectGroupAndRestoreFavoriteGroups();
    }

    private async Task DeleteSelectedActivity(IList<object> selectedItems)
    {
        if (!selectedItems.Any()) return;

        foreach (string group in selectedItems)
        {
            await _dbTransferService.DeleteActivity(group);
        }

        // restore Activity is necessary
        await BackupCurrentSelectActivityListAndRestoreSelected();
    }

    private async Task BackupCurrentSelectGroupAndRestoreFavoriteGroups()
    {
        var currentGroup = string.Empty;

        if (FavoriteGroups.Any())
        {
            currentGroup = FavoriteGroups[SelectedFavGroupIndex];
        }

        // restore favorite group comboBox
        await RestoreFavoriteGroups();

        if (FavoriteGroups.Any())
        {
            // will trigger FavoriteListSelectionChanged
            var index = FavoriteGroups.IndexOf(currentGroup);
            // user may delete a group displayed currently
            SelectedFavGroupIndex = index > 0 ? index : 0;
        }
    }

    public async Task AddToFavorite(string groupName, string codes)
    {
        var accuracyCodes = StringHelper.FormatInputCode(codes);

        if (!accuracyCodes.Any()) return;

        foreach (var accuracyCode in accuracyCodes)
        {
            await _dbTransferService.AddToFavorite(accuracyCode, groupName);
        }

        await BackupCurrentSelectGroupAndRestoreFavoriteGroups();
    }

    private void OnClearDataGrid()
    {
        AnalyzedResults.Clear();
    }

    private async Task OnAnalysisPressed()
    {
        SetCurrentStatus("开始执行。。。");

        // check code accuracy
        var accuracyCodes = StringHelper.FormatInputCode(CodeInput);
        // check null input
        if (accuracyCodes == null) return;

        // add comma
        var pyArg = StockService.FormatPyArgument(accuracyCodes);

        SetCurrentStatus("检查输入。。。");

        // formatted code resetting
        CodeInput = pyArg;

        var pyPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.PythonInstallationPath);

        if (string.IsNullOrEmpty(pyPath))
        {
            SetCurrentStatus("ERROR=>python路径未设置，任务终止");
            return;
        }

        var tdxPath = await _localSettingsService.ReadSettingAsync<string>(PjConstant.TdxInstallationPath);

        if (string.IsNullOrEmpty(tdxPath))
        {
            SetCurrentStatus("ERROR=>tdx路径未设置，任务终止");
            return;
        }

        var pyExecPath = Path.Combine(pyPath, PythonService.PyName);

        //const string pyScriptPath = "C:\\Workspace\\WK\\blk test\\tdx_reader.py";
        var pyScriptPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase!, PythonService.PyFolder, PythonService.TdxReader);

        var f = await StorageFile.GetFileFromPathAsync(pyScriptPath);

        if (f == null)
        {
            SetCurrentStatus("无法找到python脚本。。。");
            return;
        }

        SetCurrentStatus("开始执行python脚本。。。");

        // python script execution
        await PythonService.RunPythonScript(pyExecPath
             , pyScriptPath
             , tdxPath
             , StartupService.PyDataPath
             , IsQueryAllChecked ? string.Empty : pyArg);

        // TODO remove this after release
        // await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath));

        SetCurrentStatus("py脚本执行完毕，开始将数据导入至本地数据库");

        // write to sqlite database
        await Task.Run(async () =>
        {
            await _dbTransferService.TransferCsvDataToDb(accuracyCodes, IsQueryAllChecked);
        });

        //store last queried  codes
        LastQueriedCodes = accuracyCodes;

        SetCurrentStatus("保存历史查询数据。。。");

        // store this activity
        await Task.Run(async () =>
        {
            await _dbTransferService.StoreActivityDataToDb(DateTime.Now, pyArg, string.Empty);
        });

        SetCurrentStatus("开始分析数据。。。");

        // invoke analyze
        await OnLastingDaysValueChanged();

        SetCurrentStatus("完成。。。");

        // query history update
        await BackupCurrentSelectActivityListAndRestoreSelected();
    }

    public async Task OnLastingDaysValueChanged()
    {
        // if none int
        if (!int.TryParse(LastingDays, out var duration)) return;

        // ignore less than 5 days analyze
        if (duration < 5) return;

        // clear preview result
        AnalyzedResults.Clear();

        if (LastQueriedCodes == null) return;

        // analyze start
        foreach (var code in LastQueriedCodes)
        {
            // analyze data
            await _stockAnalyzeService.Analyze(code, duration, OnStockAnalyzeFinishedCallBack);
        }
    }

    /// <summary>
    /// when analyze finished, pass result to ui
    /// </summary>
    /// <param name="data"></param>
    private void OnStockAnalyzeFinishedCallBack(AnalysisResult data)
    {
        AnalyzedResults.Add(data);
    }

    public async Task RestoreFavoriteGroups()
    {
        var groups = (await _restoreSettingsService.GetFavoriteGroupsName()).ToList();

        FavoriteGroups.Clear();

        if (!groups.Any())
        {
            FavoriteGroups.Add("自选股");
        }
        else
        {
            foreach (var group in groups) FavoriteGroups.Add(group);
        }
    }

    private async Task RefreshFavoriteGroup(string selectedGroup)
    {

        var favorites = _restoreSettingsService.RestoreFavoriteCodesString(selectedGroup);

        GroupList.Clear();

        await foreach (var favorite in favorites)
        {
            GroupList.Add(favorite);
        }
    }

    public async Task RestoreLastQueryString()
    {
        // get service
        var restoreSettingsService = App.GetService<IRestoreSettingsService>();
        // query from db
        var lastQuery = await restoreSettingsService.RestoreLastQueryStringFromDb();

        if (lastQuery != null) CodeInput = lastQuery;
    }

    public async Task RestoreActivity()
    {
        // get all activity date
        var activityDates = _restoreSettingsService.RestoreRecentlyActivityGroupByDate();
        // clear first is needed
        ActivityDateList.Clear();

        await foreach (var activityDate in activityDates)
        {
            // add to list
            ActivityDateList.Add(activityDate);
        }

        if (!ActivityDateList.Any())
        {
            // if no data, add today
            ActivityDateList.Add(DateTime.Today.ToString(PjConstant.DateFormatUsedToCompare));
        }
    }

    private async Task RefreshActivityList(string date)
    {
        if (SelectedActivityDate == -1) return;

        var activityDates = _restoreSettingsService.RestoreRecentlyActivityListByDate(date);

        //clear first
        ActivityDates.Clear();

        await foreach (var activityDate in activityDates)
        {
            // add to list
            ActivityDates.Add(activityDate);
        }
    }

    private async Task BackupCurrentSelectActivityListAndRestoreSelected()
    {
        var currentActivityDate = string.Empty;

        if (ActivityDateList.Any())
        {
            currentActivityDate = ActivityDateList[SelectedActivityDate];
        }

        // restore favorite group comboBox
        await RestoreActivity();

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
        await RefreshFavoriteGroup(FavoriteGroups[SelectedFavGroupIndex]);
    }

    public async Task QueryHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedActivityDate == -1) return;
        await RefreshActivityList(ActivityDateList[SelectedActivityDate]);
    }

    public void ActivityDateClicked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not ActivityData activityData) return;

        CodeInput = activityData.QueryString;
    }

    private void SetCurrentStatus(string status)
    {
        CurrentStatus = status;
    }
}