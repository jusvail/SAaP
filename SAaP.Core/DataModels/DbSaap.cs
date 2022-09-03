using System;

using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;
using SAaP.Core.Models.DB;

namespace DataModels
{
    /// <summary>
    /// Database       : saap
    /// Data Source    : saap
    /// Server Version : 3.24.0
    /// </summary>
    public partial class DbSaap : LinqToDB.Data.DataConnection
    {

        public ITable<Stock> Stock => this.GetTable<Stock>();

        public ITable<OriginalData> OriginalData => this.GetTable<OriginalData>();

        public DbSaap(string dbPath) : base(LinqToDB.ProviderName.SQLite, dbPath)
        {
            InitDataContext();
            InitMappingSchema();
        }

        partial void InitDataContext();
        partial void InitMappingSchema();
    }
}
