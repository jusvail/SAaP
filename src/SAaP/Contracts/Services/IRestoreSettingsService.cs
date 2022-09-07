using SAaP.Core.Models;

namespace SAaP.Contracts.Services;

public interface IRestoreSettingsService
{
    Task<string> RestoreLastQueryStringFromDb();

    IAsyncEnumerable<FavoriteDetail> RestoreFavoriteCodesString(string group);

    Task<Dictionary<string, IEnumerable<FavoriteDetail>>> RestoreAllFavoriteGroupsString();

    Task<IEnumerable<string>> GetFavoriteGroupsName();
}