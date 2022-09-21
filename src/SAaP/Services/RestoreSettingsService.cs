using SAaP.Constant;
using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Models;
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

    public async IAsyncEnumerable<FavoriteDetail> RestoreFavoriteCodesString(string groupName)
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var result = from f in db.Favorite
                     join s in db.Stock
                         on new { a = f.Code, b = f.BelongTo } equals new { a = s.CodeName, b = s.BelongTo }
                     where f.GroupName == groupName
                     orderby s.CodeName
                     select new FavoriteDetail()
                     {
                         CodeName = f.BelongTo + f.Code,
                         BelongTo = f.BelongTo,
                         CompanyName = s.CompanyName,
                         GroupId = f.Id,
                         GroupName = f.GroupName
                     };

        foreach (var favoriteDetail in result)
        {
            yield return favoriteDetail;
        }
    }

    public async IAsyncEnumerable<string> RestoreRecentlyActivityGroupByDate()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);
        // query from db
        var dates = db.ActivityData.Select(a => a)
            .OrderByDescending(a => a.Date);

        var existDate = new List<string>();

        foreach (var activityData in dates)
        {
            // full date stored in db
            // 2022-09-09 14:32:18.3684624
            var data = activityData.Date.ToString(PjConstant.DateFormatUsedToCompare);
            // exist =>skip
            if (existDate.Contains(data)) continue;
            // keep tmp
            existDate.Add(data);
            // return fresh new one
            yield return data;
        }
    }

    public async IAsyncEnumerable<ActivityData> RestoreRecentlyActivityListByDate(string day)
    {
        // don't pass a strange stuff
        if (!DateTime.TryParse(day, out var thisDay)) yield break;

        var dayOfTomorrow = thisDay.AddDays(1.0);

        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);
        // query from db
        var datas = db.ActivityData
            .Where(a => a.Date > thisDay && a.Date < dayOfTomorrow)
            .OrderByDescending(a => a.Date);

        // no data return
        if (!datas.Any()) yield break;

        // each and return
        foreach (var activityData in datas)
        {
            yield return activityData;
        }
    }

    public async Task<Dictionary<string, IEnumerable<FavoriteDetail>>> RestoreAllFavoriteGroupsString()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var favoriteDetails = from f in db.Favorite
                              join s in db.Stock on f.Code equals s.CodeName
                              orderby s.CodeName
                              select new FavoriteDetail()
                              {
                                  CodeName = f.Code,
                                  CompanyName = s.CompanyName,
                                  GroupId = f.Id,
                                  GroupName = f.GroupName
                              };

        var groups = favoriteDetails.GroupBy(f => f.GroupName, f => f);

        var dic = new Dictionary<string, IEnumerable<FavoriteDetail>>();

        foreach (var group in groups)
        {
            dic.Add(group.Key, group.ToList());
        }

        return dic;
    }

    public async Task<IEnumerable<string>> GetFavoriteGroupsName()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        return db.Favorite.Select(f => f.GroupName).Distinct().ToList();
    }
}