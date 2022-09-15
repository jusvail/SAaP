namespace SAaP.Core.Services.Api;

public static class WebServiceApi
{
    public const string TxStockApi = "http://qt.gtimg.cn/q=";

    public const string DfPopularListStockApi = "https://emappdata.eastmoney.com/stockrank/getAllCurrentList";


    public static string GenerateTxQueryString(string codeNameFull)
    {
        return TxStockApi + codeNameFull;
    }

    public static string GenerateTxQueryString(string division, string codeName)
    {
        return TxStockApi + division + codeName;
    }
}