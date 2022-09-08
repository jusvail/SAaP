using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAaP.Core.Helpers;
using SAaP.Core.Models;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using SAaP.Contracts.Services;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;

namespace SAaP.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private bool _isQueryAllChecked;
    private string _codeInput;
    private string _lastingDays;
    private int _selectedFavGroupIndex;

    // store csv output by py script to sqlite database
    private readonly IDbTransferService _dbTransferService;
    // analyze main service
    private readonly IStockAnalyzeService _stockAnalyzeService;
    // restore  settings service
    private readonly IRestoreSettingsService _restoreSettingsService;

    public ObservableCollection<AnalysisResult> AnalyzedResults { get; } = new();

    public ObservableCollection<string> FavoriteGroups { get; } = new();

    public ObservableCollection<string> FavoriteGroupsWhichReadyToDelete { get; } = new();

    public ObservableCollection<FavoriteDetail> GroupList { get; } = new();

    private IList<string> LastQueriedCodes { get; set; }

    public IAsyncRelayCommand AnalysisPressedCommand { get; }

    public IRelayCommand ClearDataGrid { get; }

    public IAsyncRelayCommand<IList<object>> DeleteSelectedFavoriteGroupsCommand { get; }

    public IAsyncRelayCommand<object> DeleteSelectedFavoriteCodesCommand { get; }

    public IRelayCommand<object> AddToQueryingCommand { get; }

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

    public MainViewModel()
    { }

    public MainViewModel(IDbTransferService dbTransferService, IStockAnalyzeService stockAnalyzeService, IRestoreSettingsService restoreSettingsService)
    {
        _dbTransferService = dbTransferService;
        _stockAnalyzeService = stockAnalyzeService;
        _restoreSettingsService = restoreSettingsService;
        AnalysisPressedCommand = new AsyncRelayCommand(OnAnalysisPressed);
        ClearDataGrid = new RelayCommand(OnClearDataGrid);
        DeleteSelectedFavoriteGroupsCommand = new AsyncRelayCommand<IList<object>>(DeleteSelectedFavoriteGroups);
        DeleteSelectedFavoriteCodesCommand = new AsyncRelayCommand<object>(DeleteSelectedFavoriteCodes);
        AddToQueryingCommand = new RelayCommand<object>(AddToQuerying);
    }

    private void AddToQuerying(object listView)
    {
        var lv = listView as ListView;

        if (lv == null) return;

        var accuracyCodes = StringHelper.FormatInputCode(CodeInput);

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
            // remove from ui list
            GroupList.Remove(favorite);
        }
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

    public async Task AddToFavorite(string groupName)
    {
        var accuracyCodes = StringHelper.FormatInputCode(CodeInput);

        if (!accuracyCodes.Any()) return;

        foreach (var accuracyCode in accuracyCodes)
        {
            await DbService.AddToFavorite(accuracyCode, groupName);
        }

        await BackupCurrentSelectGroupAndRestoreFavoriteGroups();
    }

    private void OnClearDataGrid()
    {
        AnalyzedResults.Clear();
    }

    private async Task OnAnalysisPressed()
    {
        // check code accuracy
        var accuracyCodes = StringHelper.FormatInputCode(CodeInput);
        // check null input
        if (accuracyCodes == null) return;

        // add comma
        var pyArg = StockService.FormatPyArgument(accuracyCodes);

        // formatted code resetting
        CodeInput = pyArg;

        // python script execution
        await PythonService.RunPythonScript(PythonService.TdxReader
            , "C:/devEnv/Tools/TDX"
            , StartupService.PyDataPath
            , IsQueryAllChecked ? string.Empty : pyArg);

        // TODO remove this after release
        // await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath));

        // write to sqlite database
        await _dbTransferService.TransferCsvDataToDb(accuracyCodes, IsQueryAllChecked);

        //store last queried  codes
        LastQueriedCodes = accuracyCodes;

        // store this activity
        await _dbTransferService.StoreActivityDataToDb(DateTime.Now, pyArg, string.Empty);

        // invoke analyze
        await OnLastingDaysValueChanged();
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

        if (lastQuery != null)
        {
            CodeInput = lastQuery;
        }
    }

    public async void FavoriteListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedFavGroupIndex == -1) return;
        await RefreshFavoriteGroup(FavoriteGroups[SelectedFavGroupIndex]);
    }
}