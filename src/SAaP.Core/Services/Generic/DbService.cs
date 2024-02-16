using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using LinqToDB;
using SAaP.Core.DataModels;
using SAaP.Core.Models.DB;

namespace SAaP.Core.Services.Generic;

public static class DbService
{
#if DEBUG || MONITORPAGEONLY
	private static int _takeOriginalDataCnt;
#endif
	public static async Task InitializeDatabase()
	{
		// db connection
		await using var db = new DbSaap(StartupService.DbConnectionString);

		//create all table
		try
		{
			if (db.Stock.Any()) Console.WriteLine("Stock Here");
		}
		catch (Exception)
		{
			// ignored
			await db.CreateTableAsync<Stock>();
		}

		try
		{
			if (db.OriginalData.Any()) Console.WriteLine("OriginalData Here");
		}
		catch (Exception)
		{
			// ignored
			await db.CreateTableAsync<OriginalData>();
		}

		// try
		// {
		// 	if (db.AnalyzedData.Any())
		// 	{
		// 		Console.WriteLine("AnalyzedData Here");
		// 	}
		// }
		// catch (Exception)
		// {
		// 	// ignored
		// 	await db.CreateTableAsync<AnalyzedData>();
		// }

		try
		{
			if (db.ActivityData.Any()) Console.WriteLine("ActivityData Here");
		}
		catch (Exception)
		{
			// ignored
			await db.CreateTableAsync<ActivityData>();
		}

		try
		{
			if (db.Favorite.Any()) Console.WriteLine("FavoriteData Here");
		}
		catch (Exception)
		{
			// ignored
			await db.CreateTableAsync<FavoriteData>();
		}

		try
		{
			if (db.InvestData.Any()) Console.WriteLine("InvestData Here");
		}
		catch (Exception)
		{
			// ignored
			await db.CreateTableAsync<InvestData>();
		}

		try
		{
			if (db.InvestSummaryData.Any()) Console.WriteLine("InvestSummaryData Here");
		}
		catch (Exception)
		{
			await db.CreateTableAsync<InvestSummaryData>();
		}

		try
		{
			if (db.RemindMessageData.Any()) Console.WriteLine("RemindMessageData Here");
		}
		catch (Exception)
		{
			await db.CreateTableAsync<RemindMessageData>();
			await DefaultMessageInsertAsync();
		}

		try
		{
			if (db.TrackData.Any()) Console.WriteLine("TrackData Here");
		}
		catch (Exception)
		{
			await db.CreateTableAsync<TrackData>();
			await DefaultTrackConditionInsertAsync();
			// ignored
		}
	}

	private static async Task DefaultTrackConditionInsertAsync()
	{
		// db connection
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var data1 = new TrackData
		{
			TrackName    = "近20日正溢价率>85%",
			TrackType    = TrackType.Filter,
			TrackContent = "L20D-L0D:OP%>1@85",
			TrackSummary = "近20日+1溢价率>85%(可能不包括今天[若为交易日]的数据)"
		};

		var data2 = new TrackData
		{
			TrackName    = "连续两天无溢价",
			TrackType    = TrackType.Filter,
			TrackContent = "L2D-L0D:OP<0",
			TrackSummary = "连续两交易日无溢价(可能不包括今天[若为交易日]的数据)"
		};

		var data3 = new TrackData
		{
			TrackName    = "昨日无溢价",
			TrackType    = TrackType.Filter,
			TrackContent = "L1D-L0D:OP<0",
			TrackSummary = "上个交易日无溢价(可能不包括今天[若为交易日]的数据)"
		};

		await db.BeginTransactionAsync();

		await db.InsertAsync(data1);
		await db.InsertAsync(data2);
		await db.InsertAsync(data3);

		await db.CommitTransactionAsync();
	}

	private static async Task DefaultMessageInsertAsync()
	{
		// db connection
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var date = DateTime.Now;

		var messagesFromDev = new List<string>
		{
			"From Dev: 不要冒着下跌10%的风险博取5%的收益。"
		};

		await db.BeginTransactionAsync();

		foreach (var remindMessageData in messagesFromDev.Select(message => new RemindMessageData
		         {
			         Message          = message,
			         AddedDateTime    = date,
			         ModifiedDateTime = date
		         }))
			await db.InsertAsync(remindMessageData);

		await db.CommitTransactionAsync();
	}

	public static async Task<string> SelectCompanyNameByCode(string codeName, int belongTo = -1)
	{
		if (string.IsNullOrEmpty(codeName)) return null;

		string codeSelect;
		var    belong = belongTo;

		switch (codeName.Length)
		{
			case StockService.StandardCodeLength:
				codeSelect = codeName;
				break;
			case StockService.TdxCodeLength:
				codeSelect = codeName.Substring(1, 6);
				belong     = Convert.ToInt32(codeName[..1]);
				break;
			default:
				codeSelect = codeName;
				break;
		}

		// db connection
		await using var db = new DbSaap(StartupService.DbConnectionString);

		// query from stock table
		var query = db.Stock.Where(s => s.CodeName == codeSelect);

		if (belong >= 0) query = query.Where(s => s.BelongTo == belong);

		// query from db first
		return !query.Any() ? null : query.Select(s => s.CompanyName).FirstOrDefault();
	}

	public static async Task<bool> CheckRecordExistInStock(Stock stock)
	{
		// db connection
		await using var db = new DbSaap(StartupService.DbConnectionString);

		return db.Stock.Any(s => s.CodeName == stock.CodeName && s.BelongTo == stock.BelongTo);
	}

	public static async Task<bool> CheckRecordExistInStock(OriginalData originalData)
	{
		// db connection
		await using var db = new DbSaap(StartupService.DbConnectionString);

		return db.OriginalData.Any(s => s.CodeName == originalData.CodeName && s.BelongTo == originalData.BelongTo && s.Day == originalData.Day);
	}

	public static async Task AddToFavorite(string codeName, int belongTo, string groupName)
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var favoriteDatas = db.Favorite.Select(f => f);

		// exist return
		if (favoriteDatas.Any(f => f.Code == codeName && f.BelongTo == belongTo && f.GroupName == groupName)) return;

		// default value when no data in table 
		var id = 0;
		// otherwise max value
		if (favoriteDatas.Any()) id = favoriteDatas.Max(f => f.Id) + 1;

		// use exist group id when group exist
		if (favoriteDatas.Any(f => f.GroupName == groupName))
			id = db.Favorite.Where(f => f.GroupName == groupName).Select(f => f.Id).ToList()[0];

		var favorite = new FavoriteData
		{
			Code      = codeName,
			GroupName = groupName,
			BelongTo  = belongTo,
			Id        = id
		};

		await db.InsertAsync(favorite);
	}

	// public static async Task<List<OriginalData>> TakeOriginalData(string codeName, int belong, int duration)
	// {
	// 	if (duration < 1) return null;
	//
	// 	await using var db = new DbSaap(StartupService.DbConnectionString);
	//
	// 	// query original data recently [duration]
	// 	return await db.OriginalData
	// 	               .Where(o => o.CodeName == codeName && o.BelongTo == belong)
	// 	               .OrderByDescending(o => o.Day)
	// 	               .Take(duration + 1).ToListAsync(); // +1 cause ... u know y
	// }

	public static async Task<List<OriginalData>> TakeOriginalDataFromFile(string codeName, int belong, int duration = 99999)
	{
		var ori = new List<OriginalData>();
		if (duration < 1) return ori;

		var fileName = belong switch
		{
			StockService.ShFlag => StockService.Sh + codeName + ".csv",
			StockService.SzFlag => StockService.Sz + codeName + ".csv",
			StockService.UsFlag => StockService.Us + codeName + ".csv",
			_                   => string.Empty
		};

		if (string.IsNullOrEmpty(fileName)) return ori;

		var folder = await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath);

		try
		{
			var file = await folder.TryGetItemAsync(fileName) as StorageFile;

			if (file == null) return ori;

			await foreach (var originalData in ReadOriginalDataFromFileAsync(file, codeName, belong)) ori.Add(originalData);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);Console.WriteLine(nameof(DbService) + nameof(TakeOriginalDataFromFile));
			return ori;
		}
		//await using var db = new DbSaap(StartupService.DbConnectionString);

		// query original data recently [duration]
		return ori.OrderByDescending(o => o.Day)
		          .Take(duration + 1).ToList(); // +1 cause ... u know y
	}

	public static async IAsyncEnumerable<OriginalData> ReadOriginalDataFromFileAsync(IStorageFile file,
	                                                                                 string codeName, int belong)
	{
		// read per line
		foreach (var line in await FileIO.ReadLinesAsync(file))
		{
			// line split by ','
			var lineObj = line.Split(',');
			// should be 7
			if (lineObj.Length != 7) continue;

			// initialize field
			var originalData = new OriginalData
			{
				CodeName = codeName,
				BelongTo = belong,
				Day      = lineObj[0],
				Opening  = CalculationService.TryParseStringToDouble(lineObj[1]),
				High     = CalculationService.TryParseStringToDouble(lineObj[2]),
				Low      = CalculationService.TryParseStringToDouble(lineObj[3]),
				Ending   = CalculationService.TryParseStringToDouble(lineObj[4]),
				Volume   = CalculationService.TryParseStringToInt(lineObj[5]),
				Amount   = CalculationService.TryParseStringToDouble(lineObj[6])
			};

			yield return originalData;
		}
	}

	public static async Task<List<OriginalData>> TakeOriginalData(string codeName, int belong, int duration, DateTime lastTradingDay)
	{
		if (duration < 1) return null;

#if DEBUG || MONITORPAGEONLY
		Console.WriteLine(++_takeOriginalDataCnt);
#endif

		var ori = await TakeOriginalDataFromFile(codeName, belong);

		// query original data recently [duration]
		var selected = ori
		               .Where(o => o.CodeName == codeName && o.BelongTo == belong)
		               .OrderByDescending(o => o.Day)
		               .ToList(); // +1 cause ... u know y

		return selected.Where(selectData => DateTime.Compare(Convert.ToDateTime(selectData.Day), lastTradingDay) < 0)
		               .Take(duration + 1).ToList();
	}
	
	public static async Task<int> TakeOriginalDataCount(string codeName, int belong)
	{
		var ori = await TakeOriginalDataFromFile(codeName, belong);

		// query original data recently [duration]
		var selected = ori
			.Count(o => o.CodeName == codeName && o.BelongTo == belong); // +1 cause ... u know y

		return selected;
	}

	public static async Task<List<OriginalData>> TakeOriginalDataAscending(string codeName, int belong, int duration = 99999)
	{
		//await using var db = new DbSaap(StartupService.DbConnectionString);

		var ori = await TakeOriginalDataFromFile(codeName, belong);

		// query original data recently [duration]
		//return await db.OriginalData
		return ori.Where(o => o.CodeName == codeName && o.BelongTo == belong)
		          .OrderBy(o => o.Day)
		          .Take(duration + 1).ToList(); // +1 cause ... u know y
	}

	public static async Task<List<OriginalData>> TakeOriginalData(string codeName, int belong, DateTime start,
	                                                              DateTime end)
	{
		//await using var db = new DbSaap(StartupService.DbConnectionString);

		var ori = await TakeOriginalDataFromFile(codeName, belong);

		// query original data recently [duration]
		var selected =
			ori.Where(o => o.CodeName == codeName && o.BelongTo == belong
			   ).Where(selectData => DateTime.Compare(Convert.ToDateTime(selectData.Day), start) >= 0 &&
			                         DateTime.Compare(Convert.ToDateTime(selectData.Day), end) < 0)
			   .OrderByDescending(o => o.Day)
			   .ToList(); // +1 cause ... u know y

		return selected;
	}
	
	public static async Task<List<OriginalData>> TakeOriginalDataAscending(string codeName, int belong, DateTime start,
	                                                                       DateTime end)
	{
		//await using var db = new DbSaap(StartupService.DbConnectionString);

		var ori = await TakeOriginalDataFromFile(codeName, belong);

		// query original data recently [duration]
		var selected =
			ori.Where(o => o.CodeName == codeName && o.BelongTo == belong
			   ).Where(selectData => DateTime.Compare(Convert.ToDateTime(selectData.Day), start) >= 0 &&
			                         DateTime.Compare(Convert.ToDateTime(selectData.Day), end) < 0)
			   .OrderBy(o => o.Day)
			   .ToList(); // +1 cause ... u know y

		return selected;
	}
}