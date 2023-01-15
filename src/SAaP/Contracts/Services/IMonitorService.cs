using SAaP.Core.Models;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Models;

namespace SAaP.Contracts.Services;

public interface IMonitorService
{
    IAsyncEnumerable<MinuteData> ReadMinuteDate(Stock stock, HistoryDeduceData historyDeduceData);

    MonitorReport StartDeduce(Stock stock, HistoryDeduceData historyDeduceData, List<MinuteData> minuteDatas);
}