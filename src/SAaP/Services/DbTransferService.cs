using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Services;
using Windows.Storage;
using SAaP.Core.Models.DB;
using LinqToDB;

namespace SAaP.Services;

public class DbTransferService : IDbTransferService
{

    /// <summary>
    /// transfer result from py script to sqlite db 
    /// </summary>
    /// <param name="codeNames">stock's code name</param>
    /// <returns>awaiting task</returns>
    public async Task TransferCsvDataToDb(IEnumerable<string> codeNames)
    {
        // pydata path
        var pyDataFolder = await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath);

        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // loop file via code name
        foreach (var codeName in codeNames)
        {
            // sh stock data
            var issh = await FileService.TryGetItemAsync(pyDataFolder, StockService.GetOutputNameSh(codeName));

            // last query codes store into db
            var stock = new Stock { CodeName = codeName };

            if (issh != null)
            {
                await InsertToDbIfRecordNotExist(issh, db, codeName);
                stock.BelongTo = StockService.ShFlag; // sh flag
            }

            // sz stock data
            var issz = await FileService.TryGetItemAsync(pyDataFolder, StockService.GetOutputNameSz(codeName));

            if (issz != null)
            {
                await InsertToDbIfRecordNotExist(issz, db, codeName);
                stock.BelongTo = StockService.SzFlag; //sz flag
            }

            // get company name
            var companyName = await StockService.FetchCompanyNameByCode(codeName, stock.BelongTo);
            stock.CompanyName = companyName;

            // insert only not exist
            if (!await DbService.CheckRecordExistInStock(codeName))
                // query history store into db
                await db.InsertAsync(stock);
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

    private static async Task InsertToDbIfRecordNotExist(IStorageFile file, DbSaap db, string codeName)
    {
        // read per line
        foreach (var line in await FileIO.ReadLinesAsync(file))
        {
            // line split by ','
            var lineObj = line.Split(',');
            // should be 6
            if (lineObj.Length != 6) continue;

            // query for exist
            var query = from o in db.OriginalData
                        where o.CodeName == codeName && o.Day == lineObj[0] // day column
                        select o;

            // when exist in db, continue
            if (query.Any()) continue;

            // initialize field
            var originalData = new OriginalData
            {
                CodeName = codeName,
                Day = lineObj[0],
                Opening = CalculationService.TryParseStringToDouble(lineObj[1]),
                High = CalculationService.TryParseStringToDouble(lineObj[2]),
                Low = CalculationService.TryParseStringToDouble(lineObj[3]),
                Ending = CalculationService.TryParseStringToDouble(lineObj[4]),
                Volume = CalculationService.TryParseStringToInt(lineObj[5])
            };

            // insert new record
            await db.InsertAsync(originalData);
        }
    }
}