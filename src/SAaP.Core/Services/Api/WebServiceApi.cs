namespace SAaP.Core.Services.Api;

public static class WebServiceApi
{

    //https://web.ifzq.gtimg.cn/appstock/app/minute/query?_var=min_data_sh600657&code=sh600657
    //https://web.sqt.gtimg.cn/utf8/q=sh600519
    //https://proxy.finance.qq.com/ifzqgtimg/appstock/app/dealinfo/getMingxiV2?code=sh110044&direction=1&

    public const string TxStockApi = "http://qt.gtimg.cn/q=";
    // 0: 未知
    // 1: 名字
    // 2: 代码
    // 3: 当前价格
    // 4: 昨收
    // 5: 今开
    // 6: 成交量（手）
    // 7: 外盘
    // 8: 内盘
    // 9: 买一
    // 10: 买一量（手）
    // 11-18: 买二 买五
    // 19: 卖一
    // 20: 卖一量
    // 21-28: 卖二 卖五
    // 29: 最近逐笔成交
    // 30: 时间
    // 31: 涨跌
    // 32: 涨跌%
    // 33: 最高
    // 34: 最低
    // 35: 价格/成交量（手）/成交额
    // 36: 成交量（手）
    // 37: 成交额（万）
    // 38: 换手率
    // 39: 市盈率
    // 40:
    // 41: 最高
    // 42: 最低
    // 43: 振幅
    // 44: 流通市值
    // 45: 总市值
    // 46: 市净率
    // 47: 涨停价
    // 48: 跌停价

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