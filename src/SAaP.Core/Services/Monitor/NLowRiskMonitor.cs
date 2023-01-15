using System;
using SAaP.Core.Models.Monitor;

namespace SAaP.Core.Services.Monitor;

public class NLowRiskMonitor : RiskMonitorBase
{
    public override MonitorNotification AnalyzeCurrentMinuteData()
    {
        throw new NotImplementedException();
    }
}