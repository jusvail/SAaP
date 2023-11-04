using SAaP.Core.Models;
using SAaP.Core.Models.DB;

namespace SAaP.Contracts.Services;

public interface IRestoreSettingsService
{
    Task<string> RestoreLastQueryStringFromDb();

    IAsyncEnumerable<FavoriteDetail> RestoreFavoriteCodesString(string group);

    IAsyncEnumerable<string> RestoreRecentlyActivityGroupByDate();

    IAsyncEnumerable<ActivityData> RestoreRecentlyActivityListByDate(string day);

    IAsyncEnumerable<Stock> RestoreRecentlyActivityListByAnalyzeData(string analyzeData);

    Task<Dictionary<string, IEnumerable<FavoriteDetail>>> RestoreAllFavoriteGroupsString();

    Task<IEnumerable<string>> GetFavoriteGroupsName();
}