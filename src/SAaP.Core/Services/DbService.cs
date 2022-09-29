using System;
using System.Collections.Generic;
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
        try
        {
            await db.CreateTableAsync<Stock>();
        }
        catch (Exception)
        {
            // ignored
        }
        try
        {
            await db.CreateTableAsync<OriginalData>();
        }
        catch (Exception)
        {
            // ignored
        }
        try
        {
            await db.CreateTableAsync<AnalyzedData>();
        }
        catch (Exception)
        {
            // ignored
        }
        try
        {
            await db.CreateTableAsync<ActivityData>();
        }
        catch (Exception)
        {
            // ignored
        }
        try
        {
            await db.CreateTableAsync<FavoriteData>();
        }
        catch (Exception)
        {
            // ignored
        }
        try
        {
            await db.CreateTableAsync<InvestData>();
        }
        catch (Exception)
        {
            // ignored
        }
        try
        {
            await db.CreateTableAsync<InvestSummaryData>();
        }
        catch (Exception)
        {
            // ignored
        }
        try
        {
            await db.CreateTableAsync<RemindMessageData>();
            await DefaultMessageInsertAsync();
        }
        catch (Exception)
        {
            // ignored
        }

    }

    private static async Task DefaultMessageInsertAsync()
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var date = DateTime.Now;

        var messagesFromDev = new List<string>
        {
            "From Dev: 不要冒着下跌10%的风险博取5%的收益。",
            "From Dev: 如果不知道该不该买入，那么不要买入; 如果不知道该不该卖出，那么立刻卖出。",
            "From Dev: 散户没有足够的本金去抵挡(甚至可能无限)下跌的风险。",
            "From Dev: 在A股，对于影响股价的人来说，做空是相当容易的事情。但是对于散户来说，做空只有空仓这一个选择。因此，90%的时间里空仓也许是不错的选择。"
        };

        await db.BeginTransactionAsync();

        foreach (var remindMessageData in messagesFromDev.Select(message => new RemindMessageData
        {
            Message = message,
            AddedDateTime = date,
            ModifiedDateTime = date
        }))
        {
            await db.InsertAsync(remindMessageData);
        }

        await db.CommitTransactionAsync();
    }

    public static async Task<string> SelectCompanyNameByCode(string codeName, int belongTo = -1)
    {
        if (string.IsNullOrEmpty(codeName)) return null;

        string codeSelect;
        var belong = belongTo;

        switch (codeName.Length)
        {
            case StockService.StandardCodeLength:
                codeSelect = codeName;
                break;
            case StockService.TdxCodeLength:
                codeSelect = codeName.Substring(1, 6);
                belong = Convert.ToInt32(codeName[..1]);
                break;
            default:
                return null;
        }

        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // query from stock table
        var query = db.Stock.Where(s => s.CodeName == codeSelect);

        if (belong >= 0)
        {
            query = query.Where(s => s.BelongTo == belong);
        }

        // query from db first
        return !query.Any() ? null : query.Select(s => s.CompanyName).FirstOrDefault();
    }

    public static async Task<bool> CheckRecordExistInStock(Stock stock)
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        return db.Stock.Any(s => s.CodeName == stock.CodeName && s.BelongTo == stock.BelongTo);
    }

    public static async Task AddToFavorite(string codeName, int belongTo, string groupName)
    {
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var favoriteDatas = db.Favorite.Select(f => f);

        // exist return
        if (favoriteDatas.Any(f => f.Code == codeName && f.BelongTo == belongTo && f.GroupName == groupName)) return;

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
            BelongTo = belongTo,
            Id = id
        };

        await db.InsertAsync(favorite);
    }

    public static async Task<List<OriginalData>> TakeOriginalData(string codeName, int belong, int duration)
    {
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // query original data recently [duration]
        return await db.OriginalData
                .Where(o => o.CodeName == codeName && o.BelongTo == belong)
                .OrderByDescending(o => o.Day)
                .Take(duration + 1).ToListAsync(); // +1 cause ... u know y
    }
}