using SAaP.Core.Services.Api;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LinqToDB.Common;
using SAaP.Core.Helpers;
using SAaP.Core.Models.HttpResponse;

namespace SAaP.Core.Services;

public static class StockService
{
	private const int StandardCodeLength = 6;
	private const int TdxCodeLength = 7;

	private const string ShCsvName = @"sh{0}.csv";
	private const string SzCsvName = @"sz{0}.csv";

	public const string Sh = "sh";
	public const string Sz = "sz";

	public const int ShFlag = 1;
	public const int SzFlag = 0;

	public static string GetLocByFlag(int flag) => flag switch
	{
		ShFlag => Sh,
		SzFlag => Sz,
		_ => null
	};

	public static string GetOutputNameSh(string codeName) => string.Format(ShCsvName, codeName);
	public static string GetOutputNameSz(string codeName) => string.Format(SzCsvName, codeName);

	public static IEnumerable<string> CheckStockCodeAccuracy(IEnumerable<string> inputs)
	{
		foreach (var input in inputs)
		{
			switch (input.Length)
			{
				case StandardCodeLength:
					yield return input;
					break;
				case TdxCodeLength:
					yield return input[1..]; // example: [1000024] cut first '1'
					break;
			}
		}
	}

	public static string FormatPyArgument(IEnumerable<string> args)
	{
		var sb = new StringBuilder();

		foreach (var arg in args)
		{
			sb.Append(arg).Append(',');
		}

		return sb.Length > 0 ?
			// remove last ','
			sb.Remove(sb.Length - 1, 1).ToString() : string.Empty;
	}

	public static async Task<string> FetchCompanyNameByCode(string codeName, int flag)
	{
		// query from db first
		var companyName = await DbService.SelectCompanyNameByCode(codeName);
		//if exist return
		if (!string.IsNullOrEmpty(companyName)) return companyName;

		// not exist in db so get from internet
		if (flag < 0) return await FetchCompanyNameFromInternet(codeName);

		// tx api => full request string
		var api = WebServiceApi.GenerateTxQueryString(GetLocByFlag(flag), codeName);
		// get result through http
		var result = await Http.GetStringAsync(api);

		// extract company name
		return ExtraCompanyNameFromHttpString(result);
	}

	private static async Task<string> FetchCompanyNameFromInternet(string codeName)
	{
		if (string.IsNullOrEmpty(codeName)) return string.Empty;

		// tx api => full request string
		var api = WebServiceApi.GenerateTxQueryString(GetLocByFlag(0), codeName);
		// get result through http
		var result = await Http.GetStringAsync(api);

		var companyName = ExtraCompanyNameFromHttpString(result);
		// find in sh
		if (!string.IsNullOrEmpty(companyName)) return companyName;

		// if not exist, find in sz
		api = WebServiceApi.GenerateTxQueryString(GetLocByFlag(1), codeName);

		result = await Http.GetStringAsync(api);

		companyName = ExtraCompanyNameFromHttpString(result);

		return companyName;
	}

	public static async Task<string> FetchCompanyNameByCode(string codeName)
	{
		// query from db first
		var companyName = await DbService.SelectCompanyNameByCode(codeName);
		//if exist return
		if (!string.IsNullOrEmpty(companyName)) return companyName;

		// not exist in db so get from internet
		return await FetchCompanyNameFromInternet(codeName);
	}

	private static string ExtraCompanyNameFromHttpString(string result)
	{
		// no way
		if (result == null) return null;

		// regex => get company name by grouping
		// example:
		// v_sz002309="51~ST中利~002309~6.20~6.05~6.09~422496~203040~219456~6.19~311~6.18~285~6.17~1879~6.16~398~6.15~2362~6.20~1599~6.21~1597~6.22~316~6.23~638~6.24~428~~20220905161424~0.15~2.48~6.35~6.09~6.20/422496/264897712~422496~26490~5.66~-1.97~~6.35~6.09~4.30~46.28~54.05~3.31~6.35~5.75~0.85~657~6.27~-20.61~-1.40~~~1.40~26489.7712~0.0000~0~ ~GP-A~-8.28~2.48~0.00~-167.74~-21.78~9.99~3.69~16.32~32.48~35.67~746478239~871787068~6.70~-21.62~746478239~~~-10.01~-0.32~"; 
		const string pattern = "\"\\d+~(.+?)~";
		// match by regex
		var matches = Regex.Matches(result, pattern);

		// not gonna be happen
		if (matches.IsNullOrEmpty()) return null;

		var groups = matches[0].Groups;

		return groups.Count < 2 ? null :
			// only one match result expected
			groups[1].Value;
	}

	public static async IAsyncEnumerable<string> PostHot100Codes()
	{
		// {"appId":"appId01","globalId":"786e4c21-70dc-435a-93bb-38","marketType":"","pageNo":1,"pageSize":100}
		const string postArgs = "{\"appId\":\"appId01\",\"globalId\":\"786e4c21-70dc-435a-93bb-38\",\"marketType\":\"\",\"pageNo\":1,\"pageSize\":100}";

		// acquire hot 100 data using post
		var result = await Http.PostStringAsync(WebServiceApi.DfPopularListStockApi, postArgs);
		// Deserialize Object
		var response = await Json.ToObjectAsync<EmResponse<EmHotData>>(result);

		foreach (var emHotData in response.Data)
		{
			yield return emHotData.Sc;
		}
	}
}