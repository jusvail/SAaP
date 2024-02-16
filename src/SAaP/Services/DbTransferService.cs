using Windows.Storage;
using LinqToDB;
using Mapster;
using SAaP.Contracts.Services;
using SAaP.Core.DataModels;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;
using SAaP.Models;

// ReSharper disable PossibleMultipleEnumeration

namespace SAaP.Services;

public class DbTransferService : IDbTransferService
{
	/// <summary>
	///     transfer result from py script to sqlite db
	/// </summary>
	/// <param name="codeNames">stock's code name</param>
	/// <param name="isQueryAll"></param>
	/// <returns>awaiting task</returns>
	public async Task TransferCsvDataToDbAsync(IEnumerable<string> codeNames, bool isQueryAll = false)
	{
		// py data path
		var pyDataFolder = await StorageFolder.GetFolderFromPathAsync(StartupService.PyDataPath);

		// will be insert into OriginalData
		//IAsyncEnumerable<OriginalData> insertionList = null;

		if (isQueryAll)
		{
			var files = await pyDataFolder.GetItemsAsync();

			if (files == null) return;

			foreach (var storageItem in files)
			{
				var file = (StorageFile)storageItem;
				var tmpName = file.Name;

				if (tmpName.Length != 12) continue;

				// input ------- usAPLE.CSV
				// input ----- sh000001.CSV
				// output ---- APLE ...
				var codeName = tmpName[2..][4..];

				// last query codes store into db
				var stock = new Stock { CodeName = codeName, BelongTo = -1 };

				if (tmpName.StartsWith(StockService.Sh))
					stock.BelongTo = StockService.ShFlag; // sh flag
				else if (tmpName.StartsWith(StockService.Sz)) stock.BelongTo = StockService.SzFlag; //sz flag

				// insertionList = GetInsertionListWhichNotExistInDb(file, stock);

				// get company name
				var companyName = await StockService.FetchCompanyNameByCode(codeName, stock.BelongTo);
				stock.CompanyName = companyName;

				try
				{
					// db connection
					await using var db = new DbSaap(StartupService.DbConnectionString);

					// // transaction
					// await db.BeginTransactionAsync();

					// insert into db
					//await foreach (var original in insertionList) await db.InsertOrReplaceAsync(original);

					// insert only not exist
					if (!await DbService.CheckRecordExistInStock(stock))
						// query history store into db
						await db.InsertAsync(stock);
					//
					// await db.CommitTransactionAsync();
				}
				catch (Exception)
				{
					// ignore db insertion error
				}
			}
		}
		else
		{
			try
			{

				await using var db = new DbSaap(StartupService.DbConnectionString);
				await db.BeginTransactionAsync();
				// loop file via code name
				foreach (var codeName in codeNames)
				{
					var belong = await App.GetService<IFetchStockDataService>().TryGetBelongByCode(codeName);

					var codeMain = codeName.Length switch
					{
						StockService.StandardCodeLength => codeName,
						StockService.TdxCodeLength => codeName.Substring(1, 6),
						_ => codeName
					};

					// last query codes store into db
					var stock = new Stock { CodeName = codeMain, BelongTo = belong };

					// get company name
					var companyName = belong == StockService.UsFlag ? codeMain : await StockService.FetchCompanyNameByCode(codeMain, stock.BelongTo);
					stock.CompanyName = companyName;
					stock.BelongTo = belong;

					// query history store into db
					await db.InsertOrReplaceAsync(stock);
				}
				await db.CommitTransactionAsync();
			}
			catch (Exception e)
			{
				Console.WriteLine(e); Console.WriteLine(GetType());
			}
		}
	}

	public async Task StoreActivityDataToDb(ActivityData activity)
	{
		// don't pass a null
		if (activity == null) return;
		// db connection
		await using var db = new DbSaap(StartupService.DbConnectionString);
		// insert async
		await db.InsertAsync(activity);
	}

	public async Task StoreActivityDataToDb(DateTime now, string queryString, string data)
	{
		// construct a new object
		var activityData = new ActivityData
		{
			Date = now,
			QueryString = queryString,
			AnalyzeData = data
		};
		// insert async
		await StoreActivityDataToDb(activityData);
	}

	public async Task DeleteFavoriteGroups(string group)
	{
		if (string.IsNullOrEmpty(group)) return;

		await using var db = new DbSaap(StartupService.DbConnectionString);

		var ready = db.Favorite.Where(f => f.GroupName == group);

		if (!ready.Any()) return;

		foreach (var deleteData in ready) await db.DeleteAsync(deleteData);
	}

	public async Task DeleteActivity(string date)
	{
		// don't pass a strange stuff
		if (!DateTime.TryParse(date, out var thisDay)) return;

		var dayOfTomorrow = thisDay.AddDays(1.0);

		await using var db = new DbSaap(StartupService.DbConnectionString);

		var ready = db.ActivityData.Where(a =>
											  a.Date > thisDay && a.Date < dayOfTomorrow && string.IsNullOrEmpty(a.AnalyzeData));

		if (!ready.Any()) return;

		foreach (var deleteData in ready) await db.DeleteAsync(deleteData);
	}

	public async Task DeleteActivityByAnalyzeData(string date)
	{
		// don't pass a strange stuff
		if (string.IsNullOrEmpty(date)) return;

		await using var db = new DbSaap(StartupService.DbConnectionString);

		var ready = db.ActivityData.Where(a => a.AnalyzeData == date);

		if (!ready.Any()) return;

		foreach (var deleteData in ready) await db.DeleteAsync(deleteData);
	}

	public async Task DeleteFavoriteCodes(FavoriteData favorite)
	{
		if (favorite == null) return;

		await using var db = new DbSaap(StartupService.DbConnectionString);

		await db.DeleteAsync(favorite);
	}

	public async Task AddToFavorite(string codeName, string groupName)
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var fetchStockDataService = App.GetService<IFetchStockDataService>();
		var belong = await fetchStockDataService.TryGetBelongByCode(codeName);
		var codeMain = StockService.CutStockCodeLen7ToLen6(codeName);

		var belong1 = belong;
		var existInStock = db.Stock.Where(s => s.CodeName == codeMain && s.BelongTo == belong1);

		// add if not exist in stock
		if (!existInStock.Any())
		{
			var stock = new Stock { CodeName = codeMain, BelongTo = belong };

			var companyName = await StockService.FetchCompanyNameByCode(codeMain, belong);

			stock.BelongTo = belong;
			stock.CompanyName = companyName;

			await db.InsertAsync(stock);
		}

		// add to favorite table
		await DbService.AddToFavorite(codeMain, belong, groupName);
	}

	public async Task SaveToInvestSummaryDataToDb(ObservableInvestSummaryDetail data)
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var summaryData = data.Adapt<InvestSummaryData>();

		try
		{
			if (data.TradeIndex > 0)
			{
				await db.UpdateAsync(summaryData);
			}
			else
			{
				await db.InsertAsync(summaryData);
				var newIndex = await db.InvestSummaryData.MaxAsync(s => s.TradeIndex);
				data.TradeIndex = newIndex;
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e); Console.WriteLine(GetType());
			throw;
		}
	}

	public async Task DeleteInvestSummaryData(ObservableInvestSummaryDetail data)
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var summaryData = data.Adapt<InvestSummaryData>();

		try
		{
			await db.BeginTransactionAsync();

			var readyToDelete = db.InvestData.Where(i => i.TradeIndex == summaryData.TradeIndex);

			foreach (var del in readyToDelete) await db.DeleteAsync(del);

			await db.DeleteAsync(summaryData);

			await db.CommitTransactionAsync();
		}
		catch (Exception e)
		{
			Console.WriteLine(e); Console.WriteLine(GetType());
			throw;
		}
	}

	public async Task SaveToInvestDataToDb(ObservableInvestSummaryDetail summaryDetail,
										   IEnumerable<ObservableInvestDetail> investDetail)
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var index = summaryDetail.TradeIndex;

		await db.BeginTransactionAsync();

		var readyToDelete = db.InvestData.Where(i => i.TradeIndex == index);

		foreach (var del in readyToDelete) await db.DeleteAsync(del);

		foreach (var data in investDetail)
		{
			var detail = data.Adapt<InvestData>();
			detail.TradeIndex = index;
			await db.InsertAsync(detail);
		}

		await db.CommitTransactionAsync();
	}

	public async IAsyncEnumerable<InvestSummaryData> SelectInvestSummaryData()
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var investSummaryDatas =
			db.InvestSummaryData
			  .Select(i => i)
			  .OrderByDescending(o => o.TradeIndex)
			  .ToList();

		foreach (var summaryData in investSummaryDatas) yield return summaryData;
	}

	public async IAsyncEnumerable<InvestData> SelectInvestDataByIndex(int index)
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var investDatas =
			db.InvestData
			  .Where(i => i.TradeIndex == index)
			  .OrderBy(i => i.TradeDate)
			  .ThenBy(i => i.TradeTime)
			  .ToList();

		foreach (var investData in investDatas) yield return investData;
	}

	public async Task AddNewReminder(string content)
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		var remindMessageData = new RemindMessageData
		{
			Message = content,
			ModifiedDateTime = DateTime.Now,
			AddedDateTime = DateTime.Now
		};

		await db.InsertAsync(remindMessageData);
	}

	public async Task DeleteReminder(RemindMessageData message)
	{
		try
		{
			await using var db = new DbSaap(StartupService.DbConnectionString);

			await db.DeleteAsync(message);
		}
		catch (Exception)
		{
			// ignored
		}
	}

	public async Task UpdateReminder(RemindMessageData message)
	{
		try
		{
			await using var db = new DbSaap(StartupService.DbConnectionString);

			message.ModifiedDateTime = DateTime.Now;

			await db.UpdateAsync(message);
		}
		catch (Exception)
		{
			// ignored
		}
	}

	public async IAsyncEnumerable<RemindMessageData> SelectReminder()
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);
		var list =
			db.RemindMessageData
			  .Select(a => a)
			  .OrderByDescending(r => r.ModifiedDateTime)
			  .ToList();

		foreach (var remindMessageData in list) yield return remindMessageData;
	}

	public async IAsyncEnumerable<Stock> SelectAllLocalStoredCodes()
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		foreach (var stock in db.Stock.Where(s => !string.IsNullOrEmpty(s.CompanyName))) yield return stock;
	}

	public async IAsyncEnumerable<Stock> SelectStockOnHold()
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		foreach (var summaryData in db.InvestSummaryData.Where(s => !s.IsArchived))
			yield return new Stock
			{
				CodeName = summaryData.CodeName[1..],
				BelongTo = Convert.ToInt32(summaryData.CodeName[..1]),
				CompanyName = summaryData.CompanyName
			};
	}

	public async IAsyncEnumerable<TrackData> SelectTrackData()
	{
		await using var db = new DbSaap(StartupService.DbConnectionString);

		foreach (var trackData in db.TrackData.Select(t => t)) yield return trackData;
	}

	public async Task InsertTrackData(TrackData data)
	{
		if (data == null) return;

		try
		{
			await using var db = new DbSaap(StartupService.DbConnectionString);
			if (data.TrackIndex < 0)
				await db.InsertAsync(data);
			else
				await db.UpdateAsync(data);
		}
		catch (Exception e)
		{
			Console.WriteLine(e); Console.WriteLine(GetType());
		}
	}

	public async Task DeleteTrackData(TrackData data)
	{
		if (data == null) return;

		try
		{
			await using var db = new DbSaap(StartupService.DbConnectionString);
			await db.DeleteAsync(data);
		}
		catch (Exception e)
		{
			Console.WriteLine(e); Console.WriteLine(GetType());
			throw;
		}
	}

	private static async IAsyncEnumerable<OriginalData> GetInsertionListWhichNotExistInDb(IStorageFile file,
																						  Stock stock)
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
				CodeName = stock.CodeName,
				BelongTo = stock.BelongTo,
				Day = lineObj[0],
				Opening = CalculationService.TryParseStringToDouble(lineObj[1]),
				High = CalculationService.TryParseStringToDouble(lineObj[2]),
				Low = CalculationService.TryParseStringToDouble(lineObj[3]),
				Ending = CalculationService.TryParseStringToDouble(lineObj[4]),
				Volume = CalculationService.TryParseStringToInt(lineObj[5]),
				Amount = CalculationService.TryParseStringToDouble(lineObj[6])
			};

			yield return originalData;
		}
	}
}