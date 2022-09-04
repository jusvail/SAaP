using System;
using LinqToDB;
using SAaP.Core.Models.DB;
using System.Threading.Tasks;
using SAaP.Core.DataModels;

namespace SAaP.Core.Services;

public static class DbService
{
    public static async Task InitializeDatabase()
    {
        await using var db = new DbSaap(StartupService.DbConnectionString);
        try
        {
            await db.CreateTableAsync<Stock>();
            await db.CreateTableAsync<OriginalData>();
            await db.CreateTableAsync<AnalyzedData>();
        }
        catch (Exception)
        {
            //TODO error catch
            throw;
        }
    }

    public static double TryParseStringToDouble(string input)
    {
        return double.TryParse(input, out var result) ? result : 0;
    }

    public static int TryParseStringToInt(string input)
    {
        return int.TryParse(input, out var result) ? result : 0;
    }
}