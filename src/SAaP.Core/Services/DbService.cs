using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using SAaP.Core.Models.DB;
using System.Threading.Tasks;
using SAaP.Core.DataModels;
using SAaP.Core.Models;

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
        await db.CreateTableAsync<ActivityData>();
        await db.CreateTableAsync<FavoriteData>();
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

    public static async Task<bool> CheckRecordExistInStock(string code)
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        return db.Stock.Any(s => s.CodeName == code);
    }

    public static async Task<Dictionary<int, List<FavoriteDetail>>> SelectFavoriteGroups()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var query = from s in db.Stock
                    join f in db.Favorite on s.GroupId equals f.Id
                    select new FavoriteDetail();

        if (!query.Any()) return null;

        var groupBy = query.GroupBy(q => q.GroupId);

        return await groupBy.ToDictionaryAsync(g => g.Key, g => g.ToList());
    }

    public static async Task AddToFavorite(string codeName, string groupName)
    {
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var favoriteDatas = db.Favorite.Select(f => f);

        // exist return
        if (favoriteDatas.Any(f => f.Code == codeName && f.GroupName == groupName)) return;

        // default value when no data in table 
        var id = 0;
        // otherwise max value
        if (favoriteDatas.Any())
        {
            id = favoriteDatas.Max(f => f.Id) + 1;
        }

        // use exist group id when group exist
        if (favoriteDatas.Any(f => f.GroupName == groupName))
        {
            id = db.Favorite.Where(f => f.GroupName == groupName).Select(f => f.Id).ToList()[0];
        }

        var favorite = new FavoriteData
        {
            Code = codeName,
            GroupName = groupName,
            Id = id
        };

        await db.InsertAsync(favorite);
    }

    public static async Task<List<OriginalData>> TakeOriginalData(string codeName, int duration)
    {
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // query original data recently [duration]
        return await db.OriginalData
                .Where(data => data.CodeName == codeName)
                .OrderByDescending(data => data.Day)
                .Take(duration + 1).ToListAsync(); // +1 cause ... u know y
    }
}