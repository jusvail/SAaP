using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace SAaP.Core.Services.Monitor;

public class MonitorManager
{

    public List<MinuteData> MinuteDatas { get; set; } = new();

    public Stock Stock { get; set; } = new();

    public MonitorCondition Condition { get; set; } = new();

    public List<RiskMonitorBase> RiskMonitors { get; set; } = new();

    public IEnumerable<MonitorNotification> AnalyzeAMinuteData(MinuteData data)
    {
        return RiskMonitors.Select(monitor => monitor.AnalyzeCurrentMinuteData(MinuteDatas, data, new ExtraInfoOfPassData())).Where(notification => notification!= null);
    }

    public void ReceiveAMinuteData(MinuteData data)
    {
        MinuteDatas.Add(data);
    }

    public void PrepareForWork()
    {
       //TODO
    }
}