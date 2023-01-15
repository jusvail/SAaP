using SAaP.Core.Models.Monitor;

namespace SAaP.Core.Services.Monitor;

public abstract class RiskMonitorBase
{
    public abstract MonitorNotification AnalyzeCurrentMinuteData();
}