using System.Linq;
using LinqToDB;
using SAaP.Core.Models.DB;
using System.Threading.Tasks;
using SAaP.Core.DataModels;

namespace SAaP.Core.Services;

public static class DbService
{
    public static async Task InitializeDatabase()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        //create all table
        await db.CreateTableAsync<Stock>();
        await db.CreateTableAsync<OriginalData>();
        await db.CreateTableAsync<AnalyzedData>();
        await db.CreateTableAsync<LastActivity>();
        await db.CreateTableAsync<FavoriteCode>();
    }

    public static async Task<string> SelectCompanyNameByCode(string codeName)
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);
        // query from stock table
        var query = from s in db.Stock
                    where s.CodeName == codeName
                    select s;

        // query from db first
        return !query.Any() ? null : query.Select(s => s.CompanyName).FirstOrDefault();
    }
}