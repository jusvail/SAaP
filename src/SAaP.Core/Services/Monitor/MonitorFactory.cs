using SAaP.Core.Models.Monitor;
using System.Collections.Generic;
using System.Linq;

namespace SAaP.Core.Services.Monitor;

public class MonitorFactory
{
    private MonitorFactory()
    { }

    public static MonitorManager Create(List<ObservableBuyMode> buyModes)
    {
        var manager = new MonitorManager();

        foreach (var mode in buyModes.Where(mode => mode.IsChecked))
        {
            switch (mode.BuyMode)
            {
                case BuyMode.NLowRisk:
                    manager.RiskMonitors.Add(new NLowRiskMonitor()); break;
                case BuyMode.NHighRisk:
                    manager.RiskMonitors.Add(new NHighRiskMonitor()); break;
                case BuyMode.AHighRisk:
                    manager.RiskMonitors.Add(new AHighRiskMonitor()); break;
                case BuyMode.WMiddleRisk:
                    manager.RiskMonitors.Add(new WMiddleRiskMonitor()); break;
            }
        }

        return manager;
    }
}