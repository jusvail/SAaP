﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SAaP.Core.Services.Analyze;

public partial class Condition
{
    public int FromDays { get; set; }

    public int ToDays { get; set; }

    public string ConditionBody { get; set; }

    public string LeftValue { get; set; }

    public string Operator { get; set; }

    public string RightValue { get; set; }

    private Condition()
    { }

    private static readonly Dictionary<string, string> FormatCharacterDictionary = new()
    {
        {"/\\s+/",""},
        {"【","["},
        {"】","]"},
        {"，",","},
        {"。","."},
        {"：",":"},
        {"》",">"},
        {"《","<"}
    };

    // L10-TD:ZD<5&&ZD>-5&&+OPD>=8
    private const string Syntax = @"^([^&]+?:[^&]+)(&&([^&]+))*";
    private const string SyntaxMainBody = @"^([^:]+?)-([^:]+?):?([^:]+)$";
    private const string DaysMatch = @"L(\d+)D";
    private const string ConditionBodyMatch = @"([\w%+-]+)([<>=]+)([\w%+-]+)";


    private static int MatchDays(string input)
    {
        if (!Regex.Match(input, DaysMatch).Success) return 0;

        var res = Regex.Replace(input, DaysMatch, m => m.Groups[1].Success ? m.Groups[1].Value : string.Empty);
        return Convert.ToInt32(res);
    }

    private void AdaptConditionFromString(string condition)
    {
        var match = Regex.Match(condition, SyntaxMainBody);

        if (!match.Success) throw new InvalidDataException("语法错误D！");

        FromDays = MatchDays(match.Groups[1].Value);
        ToDays = MatchDays(match.Groups[2].Value);
        ConditionBody = match.Groups[3].Value;

        var matchBody = Regex.Match(ConditionBody, ConditionBodyMatch);

        if (!matchBody.Success) throw new InvalidDataException("语法错误B！");

        LeftValue = matchBody.Groups[1].Value;
        Operator = matchBody.Groups[2].Value;
        RightValue = matchBody.Groups[3].Value;
    }

    public static IEnumerable<Condition> Parse(string condition)
    {
        var match = Regex.Match(condition, Syntax);

        if (!match.Success) throw new InvalidDataException("语法错误P！");

        int from = -1, to = -1;

        for (var i = 1; i < match.Groups.Count; i++)
        {
            if (i % 2 == 0) continue;
            if (string.IsNullOrEmpty(match.Groups[i].Value)) continue;

            var con = new Condition();
            con.AdaptConditionFromString(match.Groups[i].Value);

            if (con.FromDays > 0)
            {
                from = con.FromDays;
                to = con.ToDays;
            }
            else
            {
                // yield return may cause some unexpected result
                con.FromDays = from;
                con.ToDays = to;
            }

            yield return con;
        }
    }

    public static bool TryParse(ref string condition)
    {
        var sb = new StringBuilder(condition);

        foreach (var replication in FormatCharacterDictionary)
        {
            sb.Replace(replication.Key, replication.Value);
        }

        condition = sb.ToString();

        return Regex.IsMatch(condition, Syntax);
    }
}

public partial class Condition
{
    public const string Op = "OP";

}

public  class ConditionOperator
{
    public const string G = ">";
    public const string L = "<";
    public const string E = "=";
    public const string GE = ">=";
    public const string LE = "<=";
}