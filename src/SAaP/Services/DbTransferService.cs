﻿using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Services;
using Windows.Storage;
using SAaP.Core.Models.DB;
using LinqToDB;

// ReSharper disable PossibleMultipleEnumeration

namespace SAaP.Services;

public class DbTransferService : IDbTransferService
{
    /// <summary>
    /// transfer result from py script to sqlite db 
    /// </summary>
    /// <param name="codeNames">stock's code name</param>
    /// <param name="isQueryAll"></param>
    /// <returns>awaiting task</returns>
    public async Task TransferCsvDataToDb(IEnumerable<string> codeNames, bool isQueryAll = false)
    {
        // pydata path
        var pyDataFolder = await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath);

        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // will be insert into OriginalData
        IAsyncEnumerable<OriginalData> insertionList = null;

        if (isQueryAll)
        {
            var files = await pyDataFolder.GetItemsAsync();

            if (files == null) return;

            foreach (var storageItem in files)
            {
                var file = (StorageFile)storageItem;
                var tmpName = file.Name;

                if (tmpName.Length != 12) continue;

                var codeName = tmpName.Substring(2, 6);

                // last query codes store into db
                var stock = new Stock { CodeName = codeName, BelongTo = -1 };

                if (tmpName.StartsWith(StockService.Sh))
                {
                    stock.BelongTo = StockService.ShFlag; // sh flag
                }
                else if (tmpName.StartsWith(StockService.Sz))
                {
                    stock.BelongTo = StockService.SzFlag; //sz flag
                }

                insertionList = GetInsertionListWhichNotExistInDb(file, stock);

                // get company name
                var companyName = await StockService.FetchCompanyNameByCode(codeName, stock.BelongTo);
                stock.CompanyName = companyName;

                // insert into db
                await foreach (var original in insertionList)
                {
                    await db.InsertAsync(original);
                }

                // insert only not exist
                if (!await DbService.CheckRecordExistInStock(stock))
                    // query history store into db
                    await db.InsertAsync(stock);
            }
        }
        else
        {
            var fetchStockDataService = App.GetService<IFetchStockDataService>();
            // loop file via code name
            foreach (var codeName in codeNames)
            {
                var belong = await fetchStockDataService.TryGetBelongByCode(codeName);
                string codeMain;

                switch (codeName.Length)
                {
                    case StockService.StandardCodeLength:
                        codeMain = codeName;
                        break;
                    case StockService.TdxCodeLength:
                        codeMain = codeName.Substring(1, 6);
                        break;
                    default:
                        continue;
                }

                // last query codes store into db
                var stock = new Stock { CodeName = codeMain, BelongTo = belong };

                // specific location sh / sz
                if (belong >= 0)
                {
                    var fileName = belong switch
                    {
                        StockService.ShFlag => StockService.GetOutputNameSh(codeMain),
                        StockService.SzFlag => StockService.GetOutputNameSz(codeMain),
                        _ => string.Empty
                    };

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var file = await pyDataFolder.TryGetItemAsync(fileName) as StorageFile;

                        if (file != null)
                        {
                            insertionList = GetInsertionListWhichNotExistInDb(file, stock);
                        }
                    }
                }
                else
                {
                    // sh stock data
                    var issh = await pyDataFolder.TryGetItemAsync(StockService.GetOutputNameSh(codeMain)) as StorageFile;

                    if (issh != null)
                    {
                        stock.BelongTo = StockService.ShFlag; // sh flag
                        insertionList = GetInsertionListWhichNotExistInDb(issh, stock);
                    }

                    // sz stock data
                    var issz = await pyDataFolder.TryGetItemAsync(StockService.GetOutputNameSz(codeMain)) as StorageFile;

                    if (issz != null)
                    {
                        stock.BelongTo = StockService.SzFlag; //sz flag
                        insertionList = GetInsertionListWhichNotExistInDb(issz, stock);
                    }
                }

                // insert into db
                if (insertionList != null)
                {
                    await foreach (var original in insertionList)
                    {
                        await db.InsertOrReplaceAsync(original);
                    }
                }

                // get company name
                var companyName = await StockService.FetchCompanyNameByCode(codeMain, stock.BelongTo);
                stock.CompanyName = companyName;
                stock.BelongTo = belong;

                try
                {
                    // query history store into db
                    await db.InsertOrReplaceAsync(stock);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public async Task StoreActivityDataToDb(ActivityData activity)
    {
        // don't pass a null
        if (activity == null) return;
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);
        // insert async
        await db.InsertAsync(activity);
    }

    public async Task StoreActivityDataToDb(DateTime now, string queryString, string data)
    {
        // construct a new object
        var activityData = new ActivityData()
        {
            Date = now,
            QueryString = queryString,
            AnalyzeData = data
        };
        // insert async
        await StoreActivityDataToDb(activityData);
    }

    public async Task DeleteFavoriteGroups(string group)
    {
        if (string.IsNullOrEmpty(group)) return;

        await using var db = new DbSaap(StartupService.DbConnectionString);

        var ready = db.Favorite.Where(f => f.GroupName == group);

        if (!ready.Any()) return;

        foreach (var deleteData in ready)
        {
            await db.DeleteAsync(deleteData);
        }
    }

    public async Task DeleteActivity(string date)
    {
        // don't pass a strange stuff
        if (!DateTime.TryParse(date, out var thisDay)) return;

        var dayOfTomorrow = thisDay.AddDays(1.0);

        await using var db = new DbSaap(StartupService.DbConnectionString);

        var ready = db.ActivityData.Where(a => a.Date > thisDay && a.Date < dayOfTomorrow);

        if (!ready.Any()) return;

        foreach (var deleteData in ready)
        {
            await db.DeleteAsync(deleteData);
        }
    }

    public async Task DeleteFavoriteCodes(FavoriteData favorite)
    {
        if (favorite == null) return;

        await using var db = new DbSaap(StartupService.DbConnectionString);

        await db.DeleteAsync(favorite);
    }

    private static async IAsyncEnumerable<OriginalData> GetInsertionListWhichNotExistInDb(IStorageFile file, Stock stock)
    {
        // read per line
        foreach (var line in await FileIO.ReadLinesAsync(file))
        {
            // line split by ','
            var lineObj = line.Split(',');
            // should be 6
            if (lineObj.Length != 6) continue;

            // initialize field
            var originalData = new OriginalData
            {
                CodeName = stock.CodeName,
                BelongTo = stock.BelongTo,
                Day = lineObj[0],
                Opening = CalculationService.TryParseStringToDouble(lineObj[1]),
                High = CalculationService.TryParseStringToDouble(lineObj[2]),
                Low = CalculationService.TryParseStringToDouble(lineObj[3]),
                Ending = CalculationService.TryParseStringToDouble(lineObj[4]),
                Volume = CalculationService.TryParseStringToInt(lineObj[5])
            };

            yield return originalData;
        }
    }

    public async Task AddToFavorite(string codeName, string groupName)
    {
        await using var db = new DbSaap(StartupService.DbConnectionString);

        var fetchStockDataService = App.GetService<IFetchStockDataService>();
        var belong = await fetchStockDataService.TryGetBelongByCode(codeName);
        var codeMain = StockService.CutStockCodeToSix(codeName);

        var belong1 = belong;
        var existInStock = db.Stock.Where(s => s.CodeName == codeMain && s.BelongTo == belong1);

        // add if not exist in stock
        if (!existInStock.Any())
        {
            var stock = new Stock { CodeName = codeMain, BelongTo = belong };

            var companyName = await StockService.FetchCompanyNameByCode(codeMain, belong);

            stock.BelongTo = belong;
            stock.CompanyName = companyName;

            await db.InsertAsync(stock);
        }

        // add to favorite table
        await DbService.AddToFavorite(codeMain, belong, groupName);
    }

}