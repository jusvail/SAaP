using SAaP.Core.Models.DB;
using SAaP.Models;

namespace SAaP.Contracts.Services;

public interface IDbTransferService
{
    Task TransferCsvDataToDb(IEnumerable<string> codeNames, bool isQueryAll = false);

    Task StoreActivityDataToDb(ActivityData activity);

    Task StoreActivityDataToDb(DateTime now, string queryString, string data);

    Task DeleteFavoriteGroups(string group);

    Task DeleteActivity(string date);

    Task DeleteFavoriteCodes(FavoriteData favorite);

    Task AddToFavorite(string codeName, string groupName);

    Task SaveToInvestSummaryDataToDb(ObservableInvestSummaryDetail data);

    Task DeleteInvestSummaryData(ObservableInvestSummaryDetail data);

    Task SaveToInvestDataToDb(ObservableInvestSummaryDetail summaryDetail, IEnumerable<ObservableInvestDetail> investDetail);

    IAsyncEnumerable<InvestSummaryData> SelectInvestSummaryData();

    IAsyncEnumerable<InvestData> SelectInvestDataByIndex(int index);

    Task AddNewReminder(string content);

    Task DeleteReminder(RemindMessageData message);

    Task UpdateReminder(RemindMessageData message);

    IAsyncEnumerable<RemindMessageData> SelectReminder();
}