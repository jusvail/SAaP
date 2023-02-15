using SAaP.Core.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SAaP.Core.Models.Monitor;

namespace SAaP.Core.Services.Monitor;

public class NLowRiskMonitor : RiskMonitorBase
{
    public ExtraInfoOfPassData ExtraInfo { get; set; } = new();

    public int MidVolumeADay { get; set; }
    public int MinVolumeADay { get; set; }
    public int MaxVolumeADay { get; set; }

    public double MidZfaDay { get; set; }
    public double MinZfaDay { get; set; }
    public double MaxZfaDay { get; set; }

    public int RecentlyHighVolume { get; set; }

    public bool ReadyToBuy { get; set; }

    public int ReadyToBuyIn { get; set; } = 4;

    private int _step1Index;
    public List<double> TodayMoney { get; set; } = new();
    public List<double> TodayEnding { get; set; } = new();
    public List<int> TodayVolume { get; set; } = new();
    public List<int> RecentlyVolume { get; set; } = new();
    public List<int> RecentlyVolume15 { get; set; } = new();
    public List<double> RecentlyEnding60 { get; set; } = new();
    public List<double> RecentlyEnding10 { get; set; } = new();

    public bool OnHold { get; set; }

    private int _index;

    public int TempDoNotBuyIn; // temp alert time
    public int SecondBuyIn; // 2* 3* ...

    private int _buyCount;

    public int BuyCount
    {
        get => _buyCount;
        set
        {
            _buyCount = value;
            SecondBuyIn += 45;
        }
    }

    public int SellCount { get; set; }
    public int SuccessCount { get; set; }

    public bool Step1 { get; set; }
    public bool Step2 { get; set; }
    public bool Step3 { get; set; }
    public bool Step4 { get; set; }

    public double Percent = 0.0;
    public string VolSpeedLowerMessage { get; set; }

    public int StartCount;
    private MinuteData StartData { get; set; } = new();
    private MinuteData BuyData { get; set; } = new();
    private int _buySinceMinute;
    private string _sellReason = string.Empty;

    public VolumeLevel CalcVolumeLevel(int currentVolume)
    {
        if (currentVolume < 600)
        {
            return VolumeLevel.ExtremelyLow;
        }

        if (currentVolume < 2000)
        {
            return VolumeLevel.Low;
        }

        if (currentVolume > MaxVolumeADay)
        {
            return VolumeLevel.ExtremelyHigh;
        }
        if (currentVolume <= MinVolumeADay || currentVolume < MinVolumeADay * 1.25)
        {
            return VolumeLevel.ExtremelyLow;
        }
        if (currentVolume > MinVolumeADay * 1.25 && currentVolume < MidVolumeADay * 0.75)
        {
            return VolumeLevel.Low;
        }

        if (currentVolume > MidVolumeADay * 0.75 && currentVolume < MidVolumeADay * 1.2)
        {
            return VolumeLevel.Normal;
        }
        if (currentVolume < MidVolumeADay * 2)
        {
            return VolumeLevel.Higher;
        }

        if (currentVolume < MidVolumeADay * 5)
        {
            return VolumeLevel.VeryHigh;
        }

        return currentVolume < MidVolumeADay * 9 ? VolumeLevel.ExtremelyHigh : VolumeLevel.Normal;
    }

    private bool CheckStep1(List<MinuteData> passDatas, MinuteData thisMinuteData)
    {
        var lastVolumeLevel = CalcVolumeLevel(RecentlyVolume15.Min());
        var currentVolumeLevel = CalcVolumeLevel(thisMinuteData.Volume);

        if (lastVolumeLevel is VolumeLevel.Low or VolumeLevel.ExtremelyLow or VolumeLevel.Normal
          && ((thisMinuteData.Volume > passDatas[^1].Volume * 3 || (int)lastVolumeLevel <= (int)currentVolumeLevel + 2)
            || (MidVolumeADay * 3 < thisMinuteData.Volume)))
        {
            if (CalculationService.CalcTtm(StartData.Opening, thisMinuteData.Ending) < 0)
            {
                return false;
            }

            if (RecentlyVolume.Max() < 3000)
            {
                return false;
            }

            if (CalculationService.CalcTtm(StartData.Opening, thisMinuteData.High) < 1.5)
            {
                return false;
            }

            _step1Index = _index;
            return true;

            // var tdc = passDatas.Count - StartCount;
            //
            // if (tdc > 2)
            // {
            //     var minData = passDatas.TakeLast(tdc).Min(m => m.Ending);
            //
            //     if (CalculationService.CalcTtm(minData, thisMinuteData.Ending) >= 1.8)
            //     {
            //         return true;
            //     }
            // }
        }
        return false;
    }

    private bool CheckStep2(List<MinuteData> passDatas, MinuteData thisMinuteData)
    {
        if (thisMinuteData.FullTime.Hour == 14 && thisMinuteData.FullTime.Minute >= 30)
        {
            return false;
        }

        var pc = VolSpeedLowerInMinutes(thisMinuteData);

        if (pc >= .5)
        {
            // var mr = RecentlyEnding10.Max();
            var mrh = RecentlyEnding60.Max();
            var ne = thisMinuteData.Ending;

            if (CalculationService.CalcTtm(StartData.Ending, mrh) > 6)
            {
                return false;
            }

            if (ne >= mrh)
            {
                return true;
            }

            // if (RecentlyVolume.Max() > 4000 && pc >= 0.9)
            // {
            //     return true;
            // }

            if (ne < StartData.Opening)
            {
                return false;
            }

            if (RecentlyVolume.Max() < 5000)
            {
                return false;
            }

            if (TodayMoney.Max() < 18000000)
            {
                return false;
            }

            if (thisMinuteData.Volume > 5000 && pc < 0.7)
            {
                return false;
            }

            if (CalculationService.CalcTtm(StartData.Ending, ne) > 8)
            {
                return false;
            }

            if (CalculationService.CalcTtm(StartData.Ending, ne) > 5 && thisMinuteData.FullTime.Hour > 14)
            {
                return false;
            }

            var zdfh = 100 * (mrh - ne) / mrh;

            if (zdfh < 1)
            {
                return true;
            }

            var tdh = TodayEnding.Max();
            var hzd = CalculationService.CalcTtm(StartData.Opening, tdh);
            var tzd = CalculationService.CalcTtm(StartData.Opening, ne);

            if (hzd < 3 && tzd > 0.5)
            {
                return true;
            }

            if ((hzd - tzd) / hzd > 0.5)
            {
                return true;
            }

            Percent = pc;

            #region IGNORED

            //
            // if (pc >= 0.9)
            // {
            //     return true;
            // }
            //
            // if (thisMinuteData.Zd() < 0.5 && thisMinuteData.Zd() > -0.5 && ExtraInfo.BbiList.Count > 0 && Math.Abs(thisMinuteData.Ending - ExtraInfo.BbiList[^1]) < 0.15)
            // {
            //     return true;
            // }
            //
            // if (pc >= .8 && ExtraInfo.BbiList.Count > 0 && thisMinuteData.Opening < ExtraInfo.BbiList[^1])
            // {
            //     return true;
            // }
            //
            // if (pc >= .8 && CalculationService.CalcTtm(mr, ne) < -2)
            // {
            //     return true;
            // }
            //
            // if (pc >= .7 && CalculationService.CalcTtm(mr, ne) > -2)
            // // && ExtraInfo.BbiList.Count > 0
            // // && thisMinuteData.Ending > ExtraInfo.BbiList[^1])
            // {
            //     return true;
            // }
            //
            // if (pc >= .6 && CalculationService.CalcTtm(mr, ne) > -2
            //              && ExtraInfo.BbiList.Count > 0
            //              && Math.Abs(thisMinuteData.Ending - ExtraInfo.BbiList[^1]) < 1
            //              && ((thisMinuteData.Opening > ExtraInfo.BbiList[^1] && thisMinuteData.Ending > ExtraInfo.BbiList[^1])
            //                  || (thisMinuteData.Opening < ExtraInfo.BbiList[^1] && thisMinuteData.Ending < ExtraInfo.BbiList[^1])))
            // {
            //     return true;
            // }
            //
            //
            // if (pc >= .5 && CalculationService.CalcTtm(mr, ne) < -2
            //              && ExtraInfo.BbiList.Count > 0
            //              && Math.Abs(thisMinuteData.Ending - ExtraInfo.BbiList[^1]) < 1)
            // {
            //     return true;
            // }
            #endregion
        }

        return false;
    }

    private bool CheckStep3(List<MinuteData> passDatas, MinuteData thisMinuteData)
    {
        if (_buySinceMinute >= 60)
        {
            _sellReason = "经过了60分钟未出现卖点；";
            return true;
        }

        if (thisMinuteData.Ending < StartData.Opening)
        {
            _sellReason = "涨跌<0";
            return true;
        }

        //
        // if (VolSpeedHigherInMinutes(thisMinuteData) > 50 && thisMinuteData.Zd() < 2)
        // {
        //     _sellReason = "成交量放大50倍，但涨跌<2%";
        //     return true;
        // }
        //
        // if (VolSpeedHigherInMinutes(thisMinuteData) > 40 && thisMinuteData.Zd() < 1.5)
        // {
        //     _sellReason = "成交量放大40倍，但涨跌<2%";
        //     return true;
        // }
        //
        // if (VolSpeedHigherInMinutes(thisMinuteData) > 15 && thisMinuteData.Zd() < 0.8)
        // {
        //     _sellReason = "成交量放大15倍，但涨跌<0.8%";
        //     return true;
        // }
        //
        // if (VolSpeedHigherInMinutes(thisMinuteData) > 10 && thisMinuteData.Zd() < 0.5)
        // {
        //     _sellReason = "成交量放大10倍，但涨跌<0.5%";
        //     return true;
        // }

        if (thisMinuteData.Zd() > 1.5)
        {
            _sellReason = "此分钟涨幅过高（>1.5）";
            return true;
        }

        if (ExtraInfo.BbiList.Count > 0 && thisMinuteData.Ending > ExtraInfo.BbiList[^1] + 2.8)
        {
            _sellReason = "此分钟涨幅过高（>BBI+2.8）";
            return true;
        }

        if (BuyData.Ending > 0 && CalculationService.CalcTtm(BuyData.Ending, thisMinuteData.Ending) > 2)
        {
            _sellReason = "无条件止盈 +2%";
            return true;
        }

        if (BuyData.Ending > 0 && CalculationService.CalcTtm(BuyData.Ending, thisMinuteData.Ending) < -1.5)
        {
            _sellReason = "无条件止损 -1.5%";
            return true;
        }

        return false;
    }

    public override MonitorNotification AnalyzeCurrentMinuteData(List<MinuteData> passDatas, MinuteData thisMinuteData, ExtraInfoOfPassData extraInfo)
    {
        RecentlyVolume = passDatas.TakeLast(60).Select(i => i.Volume).ToList();
        // higher the  rec
        RecentlyVolume15 = passDatas.TakeLast(15).Select(i => i.Volume).ToList();
        RecentlyEnding60 = passDatas.TakeLast(60).Select(i => i.Ending).ToList();
        RecentlyEnding10 = passDatas.TakeLast(10).Select(i => i.Ending).ToList();

        TodayVolume.Add(thisMinuteData.Volume);
        TodayEnding.Add(thisMinuteData.Ending);
        TodayMoney.Add(thisMinuteData.Ending * thisMinuteData.Volume * 10);

        CalcBbi(passDatas, thisMinuteData);

        if (_index == 0)
        {
            PrepareForWork(passDatas, extraInfo);
        }

        if (_index >= TradingMinutesInDay)
        {
            _index = 0;
            // reset
            StartData = passDatas[^1];
            StartCount = passDatas.Count;

            PrepareForWork(passDatas, extraInfo);
        }

        if (!Step1)
        {
            Step1 = CheckStep1(passDatas, thisMinuteData);
        }

        _index++;

        if (TempDoNotBuyIn > 0)
        {
            TempDoNotBuyIn--;
            return null;
        }

        if (SuccessCount > 0)
        {
            return null;
        }

        if (!Step1) return null;

        if (!Step2)
        {
            Step2 = CheckStep2(passDatas, thisMinuteData);
            if (Step2)
            {
                // TempDoNotBuyIn = 2;
                // _buySinceMinute = 2;
                BuyData = thisMinuteData;
                BuyCount++;

                var notification = new MonitorNotification
                {
                    CodeName = passDatas.First().CodeName,
                    CompanyName = passDatas.First().CompanyName,
                    Direction = DealDirection.Buy,
                    FullTime = thisMinuteData.FullTime,
                    Price = thisMinuteData.Ending,
                    SuccessPercent = Percent,
                    Positions = Percent,
                    ExpectedProfit = 2,
                    Message = VolSpeedLowerMessage,
                    SubmittedByMode = BuyMode.NLowRisk
                };

                return notification;
            }
        }

        if (!Step2) return null;

        _buySinceMinute++;

        if (!Step3)
        {
            Step3 = CheckStep3(passDatas, thisMinuteData);

            if (Step3)
            {
                // 不启用step4
                TempDoNotBuyIn = SecondBuyIn;
                Step1 = false;
                Step2 = false;
                Step3 = false;

                SellCount++;

                var notification = new MonitorNotification
                {
                    CodeName = passDatas.First().CodeName,
                    CompanyName = passDatas.First().CompanyName,
                    Direction = DealDirection.Sell,
                    FullTime = thisMinuteData.FullTime,
                    Price = thisMinuteData.Ending,
                    // SuccessPercent = pc,
                    // Positions = pc,
                    ExpectedProfit = 0,
                    HoldTime = _buySinceMinute,
                    Profit = CalculationService.Round2(CalculationService.CalcTtm(BuyData.Ending, thisMinuteData.Ending)),
                    SubmittedByMode = BuyMode.NLowRisk
                };

                notification.Message = notification.Profit.ToString(CultureInfo.InvariantCulture) + "% " + _sellReason;

                if (notification.Profit > 0)
                {
                    SuccessCount++;
                }

                BuyData = new MinuteData();
                _buySinceMinute = 0;

                return notification;
            }
        }

        return null;
    }

    private void CalcBbi(List<MinuteData> minuteDatas, MinuteData thisMinuteData)
    {
        if (minuteDatas.Count < 24) return;

        double sum3A = 0, sum6A = 0, sum12A = 0, sum24A = 0;

        var i = 1;

        var sump = 0.0;

        while (i <= 24)
        {
            sump += minuteDatas[^i].Ending;

            switch (i)
            {
                case 3:
                    sum3A = sump;
                    break;
                case 6:
                    sum6A = sump;
                    break;
                case 12:
                    sum12A = sump;
                    break;
                case 24:
                    sum24A = sump;
                    break;
            }

            i++;
        }

        var a3 = sum3A / 3;
        var a6 = sum6A / 6;
        var a12 = sum12A / 12;
        var a24 = sum24A / 24;

        ExtraInfo.BbiList.Add(CalculationService.Round3((a3 + a6 + a12 + a24) / 4));
    }

    public void PrepareForWork(List<MinuteData> passDatas, ExtraInfoOfPassData extraInfo)
    {
        //ExtraInfo = extraInfo;
        var volumes = passDatas.Select(m => m.Volume).ToList();

        if (volumes.Count <= 100) return;

        volumes.Sort();
        MidVolumeADay = volumes[volumes.Count / 2];

        var i = 0;
        while (i < volumes.Count - 1 && volumes[i] < 10) i++;

        MinVolumeADay = volumes[i];
        MaxVolumeADay = volumes[^1];

        var zf = passDatas.Select(m => (m.Ending - m.Opening) / m.Opening).ToList();

        zf.Sort();

        MidZfaDay = zf[zf.Count / 2];
        MinZfaDay = zf[0];
        MaxZfaDay = zf[^1];

        if (StartCount == 0)
        {
            StartData = passDatas[^1];
            StartCount = passDatas.Count;
        }
    }

    public double VolSpeedHigherInMinutes(MinuteData thisMinuteData)
    {
        // ReSharper disable once PossibleLossOfFraction
        return CalculationService.Round2(thisMinuteData.Volume / RecentlyVolume15.Where(v => v > 10).Min());
    }

    public double VolSpeedLowerInMinutes(MinuteData thisMinuteData)
    {
        var ml60 = RecentlyVolume.Max();
        var ml15 = RecentlyVolume15.Max();

        if (ml60 > thisMinuteData.Volume * 50)
        {
            VolSpeedLowerMessage = "60min内交易量↓30倍";
            return .95;
        }

        if (ml60 > thisMinuteData.Volume * 30)
        {
            VolSpeedLowerMessage = "60min内交易量↓30倍";
            return .9;
        }

        if (ml60 > thisMinuteData.Volume * 20)
        {
            VolSpeedLowerMessage = "60min内交易量↓20倍";
            return .8;
        }


        if (ml60 > thisMinuteData.Volume * 15)
        {
            if (thisMinuteData.Volume * thisMinuteData.Ending * 10 < 1800000)
            {
                VolSpeedLowerMessage = "60min内交易量↓15倍";
                return .7;
            }

            if (thisMinuteData.Volume < 1000)
            {
                VolSpeedLowerMessage = "60min内交易量↓15倍";
                return .7;
            }
        }

        if (TodayMoney.Max() > 20000000)
        {
            if (thisMinuteData.Volume * thisMinuteData.Ending * 10 < 1800000)
            {
                VolSpeedLowerMessage = "2kw&&现额<180w";
                return .7;
            }

            // if (RecentlyVolume.Count(d => d < 500) >= 5)
            // {
            //     VolSpeedLowerMessage = "60min内5次<500vol";
            //     return .6;
            // }
        }

        //
        // if (ml60 > thisMinuteData.Volume * 10)
        // {
        //     VolSpeedLowerMessage = "60min内交易量↓10倍";
        //     return .7;
        // }

        // if (ml60 > 9850)
        // {
        //     if (thisMinuteData.Volume < 400)
        //     {
        //         VolSpeedLowerMessage = "60min内极致缩量";
        //         return .6;
        //     }
        // }

        // if (ml60 > thisMinuteData.Volume * 9)
        // {
        //     VolSpeedLowerMessage = "60min内交易量↓9倍";
        //     return .6;
        // }
        if (ml15 > 30000 && ml15 > thisMinuteData.Volume * 4)
        {
            if (thisMinuteData.FullTime.Hour < 10)
            {
                VolSpeedLowerMessage = "巨量且15min内交易量↓4倍";
                return .5;
            }
        }

        // if (VolumeLowerInMinutes(thisMinuteData.Volume * 5))
        // {
        //     VolSpeedLowerMessage = "10min内交易量↓5倍";
        //     return .7;
        // }
        //
        // if (VolumeLowerInMinutes(thisMinuteData.Volume * 4))
        // {
        //     VolSpeedLowerMessage = "10min内交易量↓4倍";
        //     return .6;
        // }
        //
        // if (VolumeLowerInMinutes((int)(thisMinuteData.Volume * 3.5)))
        // {
        //     VolSpeedLowerMessage = "10min内交易量↓3.5倍";
        //     return .5;
        // }

        return .1;
    }
}