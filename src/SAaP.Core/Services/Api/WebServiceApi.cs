namespace SAaP.Core.Services.Api;

public static class WebServiceApi
{

    //https://web.ifzq.gtimg.cn/appstock/app/minute/query?_var=min_data_sh600657&code=sh600657
    //https://web.sqt.gtimg.cn/utf8/q=sh600519
    //https://proxy.finance.qq.com/ifzqgtimg/appstock/app/dealinfo/getMingxiV2?code=sh110044&direction=1&

    public const string TxStockApi = "http://qt.gtimg.cn/q=";

    public const string TxStockRealtimeApi = "https://web.sqt.gtimg.cn/utf8/q=";

    public const string DfPopularListStockApi = "https://emappdata.eastmoney.com/stockrank/getAllCurrentList";


    public static string GenerateTxQueryString(string codeNameFull)
    {
        return TxStockApi + codeNameFull;
    }
    public static string GenerateTxRealtimeQueryString(string codeNameFull)
    {
        return TxStockRealtimeApi + codeNameFull;
    }

    public static string GenerateTxQueryString(string division, string codeName)
    {
        return TxStockApi + division + codeName;
    }
}