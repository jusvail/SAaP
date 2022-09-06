using SAaP.Core.Models.DB;

namespace SAaP.Contracts.Services;

public interface IRestoreSettingsService
{
    Task<string> RestoreLastQueryStringFromDb();

    Task<IEnumerable<Stock>> RestoreFavoriteCodesString(string group);

    Task<Dictionary<string, IEnumerable<Stock>>> RestoreAllFavoriteGroupsString();

}