﻿using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using SAaP.Core.Models.DB;
using System.Threading.Tasks;
using LinqToDB.Data;
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
}