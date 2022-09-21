namespace SAaP.Contracts.Services;

public interface IFetchStockDataService
{
    Task FetchStockData(string pyArg, bool isCheckAll = false);

    Task<int> TryGetBelongByCode(string code);
}