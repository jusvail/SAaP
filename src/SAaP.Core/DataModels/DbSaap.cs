using LinqToDB;
using SAaP.Core.Models.DB;

namespace SAaP.Core.DataModels;

/// <summary>
/// Database       : saap
/// Data Source    : saap
/// Server Version : 3.24.0
/// </summary>
public class DbSaap : LinqToDB.Data.DataConnection
{

    public ITable<Stock> Stock => this.GetTable<Stock>();

    public ITable<OriginalData> OriginalData => this.GetTable<OriginalData>();

    public ITable<ActivityData> ActivityData => this.GetTable<ActivityData>();

    public ITable<FavoriteData> Favorite => this.GetTable<FavoriteData>();

    public ITable<InvestData> InvestData => this.GetTable<InvestData>();

    public ITable<RemindMessageData> RemindMessageData => this.GetTable<RemindMessageData>();

    public ITable<InvestSummaryData> InvestSummaryData => this.GetTable<InvestSummaryData>();

    public DbSaap(string dbPath) : base(ProviderName.SQLite, dbPath)
    { }

}