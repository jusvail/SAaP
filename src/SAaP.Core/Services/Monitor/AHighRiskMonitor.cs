using System.Collections.Generic;
using SAaP.Core.Models;
using SAaP.Core.Models.Monitor;

namespace SAaP.Core.Services.Monitor;

public class AHighRiskMonitor : RiskMonitorBase
{
    public override MonitorNotification AnalyzeCurrentMinuteData(List<MinuteData> passDatas, MinuteData thisMinuteData, ExtraInfoOfPassData extraInfo)
    {
        return null;
    }
}