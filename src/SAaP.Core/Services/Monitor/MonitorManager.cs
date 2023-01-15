using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Models;
using System.Collections.Generic;

namespace SAaP.Core.Services.Monitor;

public class MonitorManager
{

    public List<MinuteData> MinuteDatas { get; set; } = new();

    public Stock Stock { get; set; }

    public MonitorCondition Condition { get; set; }

    public List<RiskMonitorBase> RiskMonitors { get; set; }

    public IEnumerable<MonitorNotification> ReceiveAMinuteData(MinuteData data)
    {

        return null;
    }

    public void PrepareForWork()
    {
        throw new System.NotImplementedException();
    }
}