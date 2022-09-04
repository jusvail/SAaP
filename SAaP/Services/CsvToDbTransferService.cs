using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Services;
using Windows.Storage;

namespace SAaP.Services;

public class CsvToDbTransferService : ICsvToDbTransferService
{
    public async void Transfer(IEnumerable<string> codeNames)
    {
        // db connection
        await using var db = new DbSaap(StartupService.DbConnectionString);

        // pydata path
        var pyDataFolder = await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath);

        // loop file via code name
        foreach (var codeName in codeNames)
        {
            // sh stock data
            var issh = await FileService.TryGetItemAsync(pyDataFolder, StockService.GetOutputNameSh(codeName));

            if (issh != null)
            {
               foreach (var line in await FileIO.ReadLinesAsync(issh))
               {
                   var obj = line.Split(',');

                   if (obj.Length != 5) continue;


               }

            }

            // sz stock data
            var issz = await FileService.TryGetItemAsync(pyDataFolder, StockService.GetOutputNameSz(codeName));


            if (issz != null)
            {

            }
        }




    }
}