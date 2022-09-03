using System;
using System.Linq;
using Windows.Storage;
using LinqToDB;
using DataModels;
using SAaP.Core.Models.DB;
using System.Threading.Tasks;

namespace SAaP.Core.Services
{
    public static class DataAccess
    {
        public async static Task InitializeDatabase()
        {
            using var db = new DbSaap(Worker.DbConnectionString);
            try
            {
                await db.CreateTableAsync<Stock>();
                await db.CreateTableAsync<OriginalData>();
                await db.CreateTableAsync<AnalyzedData>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
