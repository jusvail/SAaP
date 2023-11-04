namespace SAaP.Core.Services.Analyze;

public static class CodeFilterFactory
{
	public static FilterBase Create(Condition condition)
	{
		return condition.LeftValue switch
		{
			Condition.Op               => new FilterOp(condition),
			Condition.Zd               => new FilterZd(condition),
			Condition.MZD              => new FilterMzd(condition),
			Condition.HZD              => new FilterHzd(condition),
			Condition.TCZT             => new FilterTczt(condition),
			Condition.NVOL             => new FilterNVol(condition),
			Condition.H_LINE_CNT       => new FilterHlc(condition),
			Condition.CROSS            => new FilterCross(condition),
			Condition.CAPI             => new FilterCapi(condition),
			Condition.CAPIL            => new FilterCapiL(condition),
			Condition.TKGK             => new FilterJumpHigh(condition),
			Condition.MZD_HZD          => new FilterMzdHzd(condition),
			Condition.OpPercent        => new FilterOpPercent(condition),
			Condition.HIS_HIGH         => new FilterHisHigh(condition),
			Condition.ISZB             => new FilterIszb(condition),
			Condition.ZXB              => new FilterZxb(condition),
			Condition.SH               => new FilterSh(condition),
			Condition.SZ               => new FilterSz(condition),
			Condition.PATTERNFIVEDAY40 => new FilterPatternFiveDay40(condition),
			Condition.PATTERNN         => new FilterPatternFoldN(condition),

			#region UN USED

			Condition.To => new FilterDayByDay(condition),
			Condition.Th => new FilterDayByDay(condition),
			Condition.Tl => new FilterDayByDay(condition),
			Condition.Te => new FilterDayByDay(condition),
			Condition.Yo => new FilterDayByDay(condition),
			Condition.Yh => new FilterDayByDay(condition),
			Condition.Yl => new FilterDayByDay(condition),
			Condition.Ye => new FilterDayByDay(condition),

			#endregion

			_ => new FilterOp(condition)
		};
	}
}