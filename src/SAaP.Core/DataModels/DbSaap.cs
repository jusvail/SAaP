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

    public DbSaap(string dbPath) : base(LinqToDB.ProviderName.SQLite, dbPath)
    {}
}