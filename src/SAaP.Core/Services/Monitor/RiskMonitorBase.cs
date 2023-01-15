using SAaP.Core.Models;
using SAaP.Core.Models.Monitor;
using System.Collections.Generic;

namespace SAaP.Core.Services.Monitor;

public abstract class RiskMonitorBase
{
    public const int TradingMinutesInDay = 240;

    public abstract MonitorNotification AnalyzeCurrentMinuteData(List<MinuteData> passDatas, MinuteData thisMinuteData,
        ExtraInfoOfPassData extraInfo);
}