﻿using SAaP.Contracts.Services;
using SAaP.Core.Models;
using SAaP.Models;
using Windows.Storage;
using Mapster;
using SAaP.Core.Models.DB;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Services.Monitor;
using SAaP.Core.Helpers;
using SAaP.Core.Services.Api;
using SAaP.Core.Services.Generic;

namespace SAaP.Services;

public class MonitorService : IMonitorService
{
	public static readonly string Stx = "min.csv";

	// const string TxRtDataPattern = "\\d+\\.\\d+";

	private static bool _noonStaff;

	public async Task RealTimeTrack(Stock stock, MonitorCondition monitorCondition, List<MinuteData> historyMinuteDatas, Action<MonitorNotification> callBack)
	{
		var manager = MonitorFactory.Create(monitorCondition.BuyModes);

		manager.Stock = stock;
		manager.Condition = monitorCondition.Adapt<MonitorCondition>();

		manager.MinuteDatas = historyMinuteDatas;

		var todayMinuteData = new List<MinuteData>();

		while (true)
		{
			while (Time.GetTimeRightNow() > Time.Mealtimes[3])
			{
				// 盘后
				break;
			}
			while (Time.GetTimeRightNow() > Time.Mealtimes[1] && Time.GetTimeRightNow() < Time.Mealtimes[2])
			{
				// 午休
				if (!_noonStaff)
				{
					callBack(MonitorNotification.SystemNotification("午休！！不要关掉窗口！"));
					_noonStaff = true;
				}
				await Task.Delay(1000);
			}

			// http receive and calculate real time data
			var sec = Time.GetTimeRightNow().Second;

			todayMinuteData.Add(new MinuteData { FullTime = Time.GetTimeOffsetRightNow() });

			var volumeThisMinute = 0;

			while (sec < 60)
			{
				try
				{
					var webString = await Http.GetStringAsync(WebServiceApi.GenerateTxQueryString(StockService.ReplaceFlagToLocString(stock.CodeName, stock.BelongTo)));

					if (string.IsNullOrEmpty(webString)) continue;

					var executed = webString.Split("~");

					if (executed.Length < 6) continue;

					// note: today's opening is [5] static
					var now = Convert.ToDouble(executed[3]);
					var vol = Convert.ToInt32(executed[6]);

					var curData = todayMinuteData[^1];

					if (curData.Opening < 1)
					{
						curData.Opening = now;
						curData.High = now;
						curData.Low = now;
						curData.Ending = now;
					}

					if (now > curData.High)
					{
						curData.High = now;
					}

					if (now < curData.Low)
					{
						curData.Low = now;
					}

					curData.Ending = now;
					volumeThisMinute = vol;

#if DEBUG
                    curData.Volume = DateTime.Now.Second * 100;
#endif
				}
				catch (Exception e)
				{
					// ignore and continue 
					await App.Logger.Log("出错了！错误信息： " + e.Message + " | " + e.StackTrace);
				}

				// 3s query once
				await Task.Delay(3000);
				sec += 3;
			}

			todayMinuteData[^1].Volume = volumeThisMinute - todayMinuteData.Sum(d => d.Volume);

			if (todayMinuteData[^1].Volume == 0) continue;

#if DEBUG

            var bt = new MonitorNotification
            {
                CodeName = "adsf",
                CompanyName = "daf",
                Price = 222.2,
                FullTime = DateTimeOffset.Now,
                Direction = DealDirection.Buy,
                SubmittedByMode = 1,
                Message = "dfaa",
            };

            callBack(bt);
            await App.Logger.Log(bt.ToString());

            callBack(MonitorNotification.SystemNotification("现在在这！！！" + stock.CompanyName));

#endif

			var notifications = manager.AnalyzeAMinuteData(todayMinuteData[^1]).ToList();
			manager.ReceiveAMinuteData(todayMinuteData[^1]);

			if (!notifications.Any()) continue;

			foreach (var notification in notifications)
			{
				callBack(notification);
				//await App.Logger.Log(notification.ToString());
			}
		}
	}

	public async Task<List<MinuteData>> ReadMinuteDateSince(Stock stock, string minuteType, DateTime since)
	{
		// minute data path
		var minDataPath = await StorageFolder.GetFolderFromPathAsync(StartupService.MinDataPath);

		var ending = "." + minuteType + Stx;

		var stockFileNamePre = StockService.ReplaceFlagToLocString(stock.CodeName, stock.BelongTo);

		var fileName = stockFileNamePre + ending;

		var csvFile = await minDataPath.TryGetItemAsync(fileName) as StorageFile;

		var datas = new List<MinuteData>();

		if (csvFile == null) return datas;

		datas.AddRange(from line in await FileIO.ReadLinesAsync(csvFile)
					   select line.Split(',')
					   into lineObj
					   where lineObj.Length == 6
					   let dateTime = DateTime.Parse(lineObj[0])
					   where dateTime >= since
					   select new MinuteData
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
					   });
		return datas;
	}

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

#if DEBUG
            if (index == 960)
            {
                Console.Write("");
            }
#endif
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

		return report;
	}

	public async IAsyncEnumerable<MinuteData> ReadMinuteDateForSimulate(Stock stock, HistoryDeduceData historyDeduceData)
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