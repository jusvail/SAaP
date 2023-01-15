namespace SAaP.Contracts.Services;

public interface IFetchStockDataService
{
    Task FetchStockData(string pyArg, bool isCheckAll = false);

    Task FetchStockMinuteData(string pyArg, int minType);

    Task<int> TryGetBelongByCode(string code);

    Task<List<string>> FormatInputCode(string input);
}