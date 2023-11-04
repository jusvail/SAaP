using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SAaP.Core.Models;
using SAaP.Core.Models.Monitor;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Monitor;

public class NHighRiskMonitor : RiskMonitorBase
{
    private int _index;

    public int Alert;

    public double Percent;

    public int StartCount;
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

    public List<int> RecentlyVolume { get; set; } = new();
    public List<int> RecentlyVolume10 { get; set; } = new();
    public List<double> RecentlyEnding60 { get; set; } = new();
    public List<double> RecentlyEnding10 { get; set; } = new();

    public bool OnHold { get; set; }


    public bool Step1 { get; set; }
    public bool Step2 { get; set; }
    public bool Step3 { get; set; }
    public bool Step4 { get; set; }
    public string VolSpeedLowerMessage { get; set; }
    public MinuteData StartData { get; set; } = new();
    public MinuteData BuyData { get; set; } = new();

    public VolumeLevel CalcVolumeLevel(int currentVolume)
    {
        if (currentVolume < 600) return VolumeLevel.ExtremelyLow;

        if (currentVolume < 2000) return VolumeLevel.Low;

        if (currentVolume > MaxVolumeADay) return VolumeLevel.ExtremelyHigh;
        if (currentVolume <= MinVolumeADay || currentVolume < MinVolumeADay * 1.25) return VolumeLevel.ExtremelyLow;
        if (currentVolume > MinVolumeADay * 1.25 && currentVolume < MidVolumeADay * 0.75) return VolumeLevel.Low;

        if (currentVolume > MidVolumeADay * 0.75 && currentVolume < MidVolumeADay * 1.2) return VolumeLevel.Normal;
        if (currentVolume < MidVolumeADay * 2) return VolumeLevel.Higher;

        if (currentVolume < MidVolumeADay * 5) return VolumeLevel.VeryHigh;

        return currentVolume < MidVolumeADay * 9 ? VolumeLevel.ExtremelyHigh : VolumeLevel.Normal;
    }

    private bool CheckStep1(List<MinuteData> passDatas, MinuteData thisMinuteData)
    {
        var lastVolumeLevel = CalcVolumeLevel(RecentlyVolume10.Min());
        var currentVolumeLevel = CalcVolumeLevel(thisMinuteData.Volume);

        if (lastVolumeLevel is VolumeLevel.Low or VolumeLevel.ExtremelyLow or VolumeLevel.Normal
            && (thisMinuteData.Volume > passDatas[^1].Volume * 3 || (int)lastVolumeLevel <= (int)currentVolumeLevel + 2
                                                                 || MidVolumeADay * 3 < thisMinuteData.Volume))
        {
            if (CalculationService.CalcTtm(StartData.Opening, thisMinuteData.Ending) < 0) return false;

            if (RecentlyVolume.Max() < 3000) return false;

            // if (RecentlyVolume10.Max() < 3000)
            // {
            //     return false;
            // }

            if (CalculationService.CalcTtm(StartData.Opening, thisMinuteData.Ending) >= 1.1) return true;

            var tdc = passDatas.Count - StartCount;

            if (tdc > 2)
            {
                var minData = passDatas.TakeLast(tdc).Min(m => m.Ending);

                if (CalculationService.CalcTtm(minData, thisMinuteData.Ending) >= 1.8) return true;
            }
        }

        return false;
    }

    private bool CheckStep2(List<MinuteData> passDatas, MinuteData thisMinuteData)
    {
        var pc = VolSpeedLowerInMinutes(thisMinuteData);

        if (pc >= .5)
        {
            var mr = RecentlyEnding10.Max();
            var ne = thisMinuteData.Ending;

            if (RecentlyVolume.Max() < 5000) return false;

            if (thisMinuteData.Volume > 5000 && pc < 0.7) return false;

            if (CalculationService.CalcTtm(StartData.Ending, ne) > 8) return false;

            if (CalculationService.CalcTtm(StartData.Ending, ne) > 5 && thisMinuteData.FullTime.Hour > 14) return false;

            Percent = pc;

            if (pc >= 0.9) return true;

            if (thisMinuteData.Zd() < 0.5 && thisMinuteData.Zd() > -0.5 && ExtraInfo.BbiList.Count > 0 &&
                Math.Abs(thisMinuteData.Ending - ExtraInfo.BbiList[^1]) < 0.15) return true;

            if (pc >= .8 && ExtraInfo.BbiList.Count > 0 && thisMinuteData.Opening < ExtraInfo.BbiList[^1]) return true;

            if (pc >= .8 && CalculationService.CalcTtm(mr, ne) < -2) return true;

            if (pc >= .7 && CalculationService.CalcTtm(mr, ne) > -2)
                // && ExtraInfo.BbiList.Count > 0
                // && thisMinuteData.Ending > ExtraInfo.BbiList[^1])
                return true;

            if (pc >= .6 && CalculationService.CalcTtm(mr, ne) > -2
                         && ExtraInfo.BbiList.Count > 0
                         && Math.Abs(thisMinuteData.Ending - ExtraInfo.BbiList[^1]) < 1
                         && ((thisMinuteData.Opening > ExtraInfo.BbiList[^1] &&
                              thisMinuteData.Ending > ExtraInfo.BbiList[^1])
                             || (thisMinuteData.Opening < ExtraInfo.BbiList[^1] &&
                                 thisMinuteData.Ending < ExtraInfo.BbiList[^1])))
                return true;


            if (pc >= .5 && CalculationService.CalcTtm(mr, ne) < -2
                         && ExtraInfo.BbiList.Count > 0
                         && Math.Abs(thisMinuteData.Ending - ExtraInfo.BbiList[^1]) < 1)
                return true;
        }

        return false;
    }

    private bool CheckStep3(List<MinuteData> passDatas, MinuteData thisMinuteData)
    {
        if (thisMinuteData.Zd() > 1.5) return true;

        if (ExtraInfo.BbiList.Count > 0 && thisMinuteData.Ending > ExtraInfo.BbiList[^1] + 2.8) return true;

        if (BuyData.Ending > 0 && CalculationService.CalcTtm(BuyData.Ending, thisMinuteData.Ending) > 2) return true;

        if (BuyData.Ending > 0 && CalculationService.CalcTtm(BuyData.Ending, thisMinuteData.Ending) < -1) return true;

        return false;
    }

    public override MonitorNotification AnalyzeCurrentMinuteData(List<MinuteData> passDatas, MinuteData thisMinuteData,
        ExtraInfoOfPassData extraInfo)
    {
        RecentlyVolume = passDatas.TakeLast(60).Select(i => i.Volume).ToList();
        // higher the  rec
        //RecentlyVolume10 = passDatas.TakeLast(15).Select(i => i.Volume).ToList();
        RecentlyVolume10 = passDatas.TakeLast(10).Select(i => i.Volume).ToList();
        RecentlyEnding60 = passDatas.TakeLast(60).Select(i => i.Ending).ToList();
        RecentlyEnding10 = passDatas.TakeLast(10).Select(i => i.Ending).ToList();

        CalcBbi(passDatas, thisMinuteData);

        if (_index == 0) PrepareForWork(passDatas, extraInfo);
        if (_index >= TradingMinutesInDay)
        {
            _index = 0;
            // reset
            StartData = passDatas[^1];
            StartCount = passDatas.Count;

            PrepareForWork(passDatas, extraInfo);
        }

        if (!Step1) Step1 = CheckStep1(passDatas, thisMinuteData);

        if (Alert > 0)
        {
            Alert--;
            return null;
        }

        if (!Step1) return null;

        if (!Step2)
        {
            Step2 = CheckStep2(passDatas, thisMinuteData);
            if (Step2)
            {
                Alert = 4;
                BuyData = thisMinuteData;

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
                    SubmittedByMode = BuyMode.NHighRisk
                };

                return notification;
            }
        }

        if (!Step2) return null;

        if (!Step3)
        {
            Step3 = CheckStep3(passDatas, thisMinuteData);

            if (Step3)
            {
                // 不启用step4
                Alert = 15;
                Step1 = false;
                Step2 = false;
                Step3 = false;

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
                    Profit = CalculationService.Round2(
                        CalculationService.CalcTtm(BuyData.Ending, thisMinuteData.Ending)),
                    SubmittedByMode = BuyMode.NLowRisk
                };

                notification.Message = notification.Profit.ToString(CultureInfo.InvariantCulture) + "%";

                BuyData = new MinuteData();

                return notification;
            }
        }

        _index++;

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

    private bool VolumeLowerInMinutes(int x)
    {
        return RecentlyVolume10.Any(vol => vol > x);
    }

    public double VolSpeedLowerInMinutes(MinuteData thisMinuteData)
    {
        if (VolumeLowerInMinutes(thisMinuteData.Volume * 20))
        {
            VolSpeedLowerMessage = "10min内交易量↓20倍";
            return .9;
        }

        if (VolumeLowerInMinutes(thisMinuteData.Volume * 10))
        {
            VolSpeedLowerMessage = "10min内交易量↓10倍";
            return .8;
        }

        if (VolumeLowerInMinutes(thisMinuteData.Volume * 5))
        {
            VolSpeedLowerMessage = "10min内交易量↓5倍";
            return .7;
        }

        if (VolumeLowerInMinutes(thisMinuteData.Volume * 4))
        {
            VolSpeedLowerMessage = "10min内交易量↓4倍";
            return .6;
        }

        if (VolumeLowerInMinutes((int)(thisMinuteData.Volume * 3.5)))
        {
            VolSpeedLowerMessage = "10min内交易量↓3.5倍";
            return .5;
        }

        return .1;
    }
}