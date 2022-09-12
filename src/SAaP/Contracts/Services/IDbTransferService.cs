using SAaP.Core.Models;
using SAaP.Core.Models.DB;

namespace SAaP.Contracts.Services
{
    public interface IDbTransferService
    {
        Task TransferCsvDataToDb(IEnumerable<string> codeNames, bool isQueryAll);

        Task StoreActivityDataToDb(ActivityData activity);

        Task StoreActivityDataToDb(DateTime now, string queryString, string data);

        Task DeleteFavoriteGroups(string group);

        Task DeleteActivity(string date);

        Task DeleteFavoriteCodes(FavoriteData favorite);

        Task AddToFavorite(string codeName, string groupName);
    }
}
