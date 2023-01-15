using SAaP.Core.Models;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Models;

namespace SAaP.Contracts.Services;

public interface IMonitorService
{
    IAsyncEnumerable<MinuteData> ReadMinuteDateForSimulate(Stock stock, HistoryDeduceData historyDeduceData);

    Task<List<MinuteData>> ReadMinuteDateSince(Stock stock, string minuteType, DateTime since);

    Task RealTimeTrack(Stock stock, MonitorCondition monitorCondition, List<MinuteData> historyMinuteDatas, Action<MonitorNotification> callBack);

    MonitorReport StartDeduce(Stock stock, HistoryDeduceData historyDeduceData, List<MinuteData> minuteDatas);
}