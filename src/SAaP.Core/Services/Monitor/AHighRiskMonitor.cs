using SAaP.Core.Models.Monitor;

namespace SAaP.Core.Services.Monitor;

public class AHighRiskMonitor : RiskMonitorBase
{
    public override MonitorNotification AnalyzeCurrentMinuteData()
    {
        throw new System.NotImplementedException();
    }
}