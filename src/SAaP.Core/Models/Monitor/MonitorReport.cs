using System.Collections.Generic;

namespace SAaP.Core.Models.Monitor
{
    public class MonitorReport
    {
        public List<MonitorNotification> Notifications { get; set; } = new();
    }
}