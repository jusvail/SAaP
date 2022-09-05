using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Services;

namespace SAaP.Services;

public class RestoreSettingsService : IRestoreSettingsService
{
    public async Task<string> RestoreLastQueryStringFromDb()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // last queried codes object
        var lastQueried = db.Stock.ToList();

        // return formatted string if exist
        return lastQueried.Any() ? StockService.FormatPyArgument(lastQueried.Select(s => s.CodeName)) : null;
    }
}