using System.Data.Common;
using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Models.DB;
using SAaP.Core.Services;

namespace SAaP.Services;

public class RestoreSettingsService : IRestoreSettingsService
{
    public async Task<string> RestoreLastQueryStringFromDb()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // last queried codes object
        var lastQueried =
            from ac in db.ActivityData
            orderby ac.Date descending
            select ac;

        // return formatted string if exist
        return lastQueried.Any() ? lastQueried.First().QueryString : string.Empty;
    }

    public async Task<IEnumerable<Stock>> RestoreFavoriteCodesString(string group)
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var favs = from s in db.Stock
            join f in db.Favorite on s.GroupId equals f.Id
                select s;

        return null;
    }

    public async Task<Dictionary<string, IEnumerable<Stock>>> RestoreAllFavoriteGroupsString()
    {
        throw new NotImplementedException();
    }
}