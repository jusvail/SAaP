using SAaP.Core.Models.Analyst;

namespace SAaP.Core.Services.Analyst;

public class FiveDay40PatternIndicator : PatternIndicator
{
	private const int Slider = 5;
	private const double MinZf = 38d;
	private const double MaxZf = 45d;

	private const double BlFixed = 2.2d;

	public override MatchResult Indicate()
	{
		var result = new MatchResult();

		// 30d内的忽略
		if (Ori.Count < 30) 
		{
			return result;
		}

		var end = Ori.Count;
		const int start = 0;

		// var range = end - start + 1;

		var x = end - 1;
		var l = x - Slider + 1;

		// step1 =>最近较短的一段时期X内，股价上涨了至少40%
		// 5个交易日内，上涨幅度位40%左右，过多过少都不好！
		var zf5 = Zd(Ori, l, x);

		//没有则返回找不到
		if (zf5 is < MinZf or > MaxZf) return result;


		// step2 最近X交易日内的成交量之和非常之大，大到的程度是从开始上涨的前一天开始向前回溯，至少是N*X个交易日的成交量之和
		// 向前回溯，直到找到一个模式 => 五日线三天呈现出v的模式

		var s = l - 1;

		while (!(s - 1 >= start && D05[s - 1] > D05[s] && D05[s] < D05[s + 1])) // 五日线三天呈现出v的模式
			s--;

		if (s <= 1) // 未找到,几乎不可能把
			return result;

		while (s < l && Zd(Ori, s, s) < 5d) // 向右寻找第一个涨跌>5%的一天
			s++;

		// 此时s为上涨第一天

		// 累加上涨以来的成交量，注意单位是元，不是E
		var sumVolumeStep1 = 0d;
		for (var i = s; i <= x; i++) sumVolumeStep1 += Ori[i].Volume;

		// step3 可选？ X内的交易量之和 < M*当前股价的市值
		var sumVolumeStep0 = 0d;
		var step0Index = s - 1;

		// 从s-1开始向前回溯，找到一个index t，使t~s-1间的成交量之和正好大于第一阶段(到今天)成交量之和，即sumVolume
		for (; step0Index >= start; step0Index--)
		{
			sumVolumeStep0 += Ori[step0Index].Volume;
			if (sumVolumeStep0 > sumVolumeStep1) break;
		}

		var step1d = x - s + 1; // 上涨开始以来的天数
		var step0d = s - 1 - step0Index;

		var bl = step0d / step1d;

		// 倍率>x，则是底部突然上涨的！放量的！
		if (bl > BlFixed) result.Bought = true;

		return result;
	}
}