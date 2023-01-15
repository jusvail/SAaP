using SAaP.Core.Models;
using SAaP.Core.Models.Monitor;
using System.Collections.Generic;

namespace SAaP.Core.Services.Monitor;

public class NHighRiskMonitor : RiskMonitorBase
{
    public override MonitorNotification AnalyzeCurrentMinuteData(List<MinuteData> passDatas, MinuteData thisMinuteData, ExtraInfoOfPassData extraInfo)
    {
        return null;
    }
}