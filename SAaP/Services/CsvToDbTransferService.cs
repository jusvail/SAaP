using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Services;
using Windows.Storage;
using SAaP.Core.Models.DB;
using LinqToDB;

namespace SAaP.Services;

public class CsvToDbTransferService : ICsvToDbTransferService
{
    public async Task Transfer(IEnumerable<string> codeNames)
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

            if (issh != null)
            {
                await InsertToDbIfRecordNotExist(issh, db, codeName, 1);
            }

            // sz stock data
            var issz = await FileService.TryGetItemAsync(pyDataFolder, StockService.GetOutputNameSz(codeName));

            if (issz != null)
            {
                await InsertToDbIfRecordNotExist(issz, db, codeName, 0);
            }
        }
    }

    private static async Task InsertToDbIfRecordNotExist(IStorageFile file, DbSaap db, string codeName, int loc)
    {
        // read per line
        foreach (var line in await FileIO.ReadLinesAsync(file))
        {
            var obj = line.Split(',');

            if (obj.Length != 6) continue;

            // query for exist
            var query = from o in db.OriginalData
                        where o.CodeName == codeName && o.Day == obj[0] // day column
                        select o;

            // when exist in db, continue
            if (query.Any()) continue;

            var od = new OriginalData
            {
                // initialize field
                CodeName = codeName,
                Day = obj[0],
                Opening = DbService.TryParseStringToDouble(obj[1]),
                High = DbService.TryParseStringToDouble(obj[2]),
                Low = DbService.TryParseStringToDouble(obj[3]),
                Ending = DbService.TryParseStringToDouble(obj[4]),
                Volume = DbService.TryParseStringToInt(obj[5])
            };

            // insert new record
            await db.InsertAsync(od);
        }
    }
}