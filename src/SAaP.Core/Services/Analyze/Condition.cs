using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// ReSharper disable IdentifierTypo

namespace SAaP.Core.Services.Analyze;

public partial class Condition
{
	// L10-TD:ZD<5&&ZD>-5&&+OPD>=8
	private const string Syntax         = @"^([^&]+?:[^&]+)(&&([^&]+))*";
	private const string SyntaxMainBody = @"^([^:]+?)-([^:]+?):?([^:]+)$";
	private const string DaysMatch      = @"L?(\d+)D?";

	private const string
		ConditionBodyMatch = @"([\w@%+\-]+)([<>=]+)([\w@%+\-]+)"; // ReSharper disable InconsistentNaming

	private static readonly Dictionary<string, string> FormatCharacterDictionary = new()
	{
		{ "/\\s+/", "" },
		{ "【", "[" },
		{ "】", "]" },
		{ "，", "," },
		{ "。", "." },
		{ "：", ":" },
		{ "》", ">" },
		{ " ", "" },
		{ "《", "<" }
	};

	private Condition()
	{
	}

	public int FromDays { get; set; }

	public int ToDays { get; set; }

	public bool IsSpecialMatch { get; set; }

	public string ConditionBody { get; set; }

	public string LeftValue { get; set; }

	public string Operator { get; set; }

	public string RightValue { get; set; }


	private static int MatchDays(string input)
	{
		if (!Regex.Match(input, DaysMatch).Success) return 0;

		var res = Regex.Replace(input, DaysMatch, m => m.Groups[1].Success ? m.Groups[1].Value : string.Empty);
		return Convert.ToInt32(res);
	}

	private void AdaptConditionFromString(string condition)
	{
		var match = Regex.Match(condition, SyntaxMainBody);

		if (match.Success)
		{
			FromDays      = MatchDays(match.Groups[1].Value);
			ToDays        = MatchDays(match.Groups[2].Value);
			ConditionBody = match.Groups[3].Value;
		}
		else
		{
			ConditionBody = condition;
		}

		// 通常匹配
		var matchBody = Regex.Match(ConditionBody, ConditionBodyMatch);

		if (matchBody is not { Success: true }) throw new InvalidDataException("语法错误B！");

		LeftValue  = matchBody.Groups[1].Value;
		Operator   = matchBody.Groups[2].Value;
		RightValue = matchBody.Groups[3].Value;
	}

	private static bool SpecialMatch(string condition)
	{
		return SpecialConditions.Contains(condition.ToUpper());
	}

	public static IEnumerable<Condition> Parse(string condition)
	{
		condition = condition.Trim().ToUpper();
		if (SpecialMatch(condition))
		{
			// 特殊匹配
			var con = new Condition
			{
				IsSpecialMatch = true,
				FromDays       = 1,
				LeftValue      = condition.ToUpper()
			};
			yield return con;
		}
		else
		{
			// 通常匹配
			var match = Regex.Match(condition, Syntax);

			if (!match.Success) throw new InvalidDataException("语法错误P！");

			int from = -1, to = -1;

			var split = condition.Split("&&");

			foreach (var t in split)
			{
				var con = new Condition();
				try
				{
					con.AdaptConditionFromString(t);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					Console.WriteLine(Parse);
					yield break;
				}

				if (con.FromDays > 0)
				{
					from = con.FromDays;
					to   = con.ToDays;
				}
				else
				{
					// yield return may cause some unexpected result
					con.FromDays = from;
					con.ToDays   = to;
				}

				yield return con;
			}
		}
	}

	public static bool TryParse(ref string condition)
	{
		var sb = new StringBuilder(condition.Trim().ToUpper());

		foreach (var replication in FormatCharacterDictionary) sb.Replace(replication.Key, replication.Value);

		condition = sb.ToString();

		return SpecialMatch(condition) || Regex.IsMatch(condition, Syntax);
	}
}

public partial class Condition
{
	public const string Op         = "OP";
	public const string Zd         = "ZD";
	public const string MZD        = "MZD";
	public const string HZD        = "HZD";
	public const string TCZT       = "CZT";
	public const string TKGK       = "TKGK";
	public const string NVOL       = "NVOL";
	public const string MZD_HZD    = "MZD-HZD";
	public const string HIS_HIGH   = "HISHIGH";
	public const string H_LINE_CNT = "HLC";
	public const string CROSS      = "CROSS";
	public const string CAPI       = "CAPI";
	public const string CAPIL      = "CAPIL";

	public const string OpPercent = "OP%";

	public const string To = "TO";
	public const string Th = "TH";
	public const string Tl = "TL";
	public const string Te = "TE";
	public const string Yo = "YO";
	public const string Yh = "YH";
	public const string Yl = "YL";
	public const string Ye = "YE";
	
	public const string ZXB              = nameof(ZXB);
	public const string ISZB             = nameof(ISZB);
	public const string ISZZ             = nameof(ISZZ);
	public const string SZ               = nameof(SZ);
	public const string SH               = nameof(SH);
	public const string PATTERNFIVEDAY40 = nameof(PATTERNFIVEDAY40);
	public const string PATTERNN         = nameof(PATTERNN);

	public const string ValueSeparator = "@";

	private static readonly List<string> SpecialConditions = new()
	{
		ISZB, ZXB, ISZZ, SZ, SH, PATTERNFIVEDAY40, PATTERNN
	};
}

public static class ConditionOperator
{
	public const string G  = ">";
	public const string L  = "<";
	public const string E  = "=";
	public const string Ge = ">=";
	public const string Le = "<=";
}