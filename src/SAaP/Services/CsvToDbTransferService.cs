using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Services;
using Windows.Storage;
using SAaP.Core.Models.DB;
using LinqToDB;

namespace SAaP.Services;

public class CsvToDbTransferService : ICsvToDbTransferService
{

    /// <summary>
    /// transfer result from py script to sqlite db 
    /// </summary>
    /// <param name="codeNames">stock's code name</param>
    /// <returns>awaiting task</returns>
    public async Task Transfer(IEnumerable<string> codeNames)
    {
        // pydata path
        var pyDataFolder = await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath);

        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // delete last queried history
        await db.Stock.DeleteAsync();

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

            // query history store into db
            await db.InsertAsync(stock);
        }
    }

    private static async Task InsertToDbIfRecordNotExist(IStorageFile file, DbSaap db, string codeName)
    {
        // read per line
        foreach (var line in await FileIO.ReadLinesAsync(file))
        {
            var lineObj = line.Split(',');

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
                Opening = DbService.TryParseStringToDouble(lineObj[1]),
                High = DbService.TryParseStringToDouble(lineObj[2]),
                Low = DbService.TryParseStringToDouble(lineObj[3]),
                Ending = DbService.TryParseStringToDouble(lineObj[4]),
                Volume = DbService.TryParseStringToInt(lineObj[5])
            };

            // insert new record
            await db.InsertAsync(originalData);
        }
    }
}