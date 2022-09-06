namespace SAaP.Core.Services.Api;

public static class WebServiceApi
{
    private const string TxStockApi = "http://qt.gtimg.cn/q=";

    public static string GenerateTxQueryString(string codeNameFull)
    {
        return TxStockApi + codeNameFull;
    }

    public static string GenerateTxQueryString(string division, string codeName)
    {
        return TxStockApi + division + codeName;
    }
}