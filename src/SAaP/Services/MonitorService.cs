using SAaP.Contracts.Services;
using SAaP.Core.Models;
using SAaP.Core.Services;
using SAaP.Models;
using Windows.Storage;
using Mapster;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Services.Monitor;

namespace SAaP.Services;

public class MonitorService : IMonitorService
{
    public static readonly string Stx = "min.csv";

    public MonitorReport StartDeduce(Stock stock, HistoryDeduceData historyDeduceData, List<MinuteData> minuteDatas)
    {
        var manager = MonitorFactory.Create(historyDeduceData.MonitorCondition.BuyModes);

        manager.Stock = stock;
        manager.Condition = historyDeduceData.MonitorCondition.Adapt<MonitorCondition>();

        var index = 0;

        // pre data 
        do
        {
            manager.MinuteDatas.Add(minuteDatas[index++]);
        } while (manager.MinuteDatas[^1].FullTime < historyDeduceData.PerLoadDateEnd && index < minuteDatas.Count);

        // pre work
        manager.PrepareForWork();

        var report = new MonitorReport();

        while (index < minuteDatas.Count - 1)
        {
            var notifications = manager.AnalyzeAMinuteData(minuteDatas[index]).ToList();
            manager.ReceiveAMinuteData(minuteDatas[index]);

            if (notifications.Any())
            {
                report.Notifications.AddRange(notifications);
            }

            index++;
        }

        if (!report.Notifications.Any())
        {
            report.Notifications.Add(new MonitorNotification
            {
                Message = "无结果"
            });
        }

        return report;
    }

    public async IAsyncEnumerable<MinuteData> ReadMinuteDate(Stock stock, HistoryDeduceData historyDeduceData)
    {
        // minute data path
        var minDataPath = await StorageFolder.GetFolderFromPathAsync(StartupService.MinDataPath);

        var ending = "." + historyDeduceData.MonitorCondition.MinuteType + Stx;

        var stockFileNamePre = StockService.ReplaceFlagToLocString(stock.CodeName, stock.BelongTo);

        var fileName = stockFileNamePre + ending;

        var csvFile = await minDataPath.TryGetItemAsync(fileName) as StorageFile;

        if (csvFile == null) yield break;

        foreach (var line in await FileIO.ReadLinesAsync(csvFile))
        {
            // line split by ','
            var lineObj = line.Split(',');
            // should be 6
            if (lineObj.Length != 6) continue;

            var dateTime = DateTimeOffset.Parse(lineObj[0]);

            if (dateTime < historyDeduceData.PreLoadDateStart || dateTime > historyDeduceData.AnalyzeEndDate)
            {
                continue;
            }

            var minuteData = new MinuteData
            {
                CodeName = stock.CodeName,
                CompanyName = stock.CompanyName,
                BelongTo = stock.BelongTo,
                FullTime = dateTime,
                Opening = CalculationService.TryParseStringToDouble(lineObj[1]),
                High = CalculationService.TryParseStringToDouble(lineObj[2]),
                Low = CalculationService.TryParseStringToDouble(lineObj[3]),
                Ending = CalculationService.TryParseStringToDouble(lineObj[4]),
                Volume = CalculationService.TryParseStringToInt(lineObj[5])
            };

            yield return minuteData;
        }
    }
}