using System;
using System.Collections.Generic;
using System.Linq;
using SAaP.Core.Models.Analyst;
using SAaP.Core.Models.DB;
using SAaP.Core.Services.Generic;

namespace SAaP.Core.Services.Analyst.Pipe;

internal static class PlumbersWife
{
    private const double CciConstant = 0.015;

    public static double CalculateAverageDayLine(IList<OriginalData> datas, int index, int lasting)
    {
        var sum = 0.0;

        for (var i = index; i >= index - lasting + 1; i--) sum += datas[i].Ending;

        return CalculationService.Round2(sum / lasting);
    }

    public static double CalculateRpsLine(IList<OriginalData> originalDatas, int i, LineForm rpsForm)
    {
        throw new NotImplementedException();
    }

    public static List<double> CalculateCciLine(IList<OriginalData> originalDatas, int period = 14,
        double constant = 0.015)
    {
        var high = originalDatas.Select(c => c.High).ToArray();
        var low = originalDatas.Select(c => c.Low).ToArray();
        var close = originalDatas.Select(c => c.Ending).ToArray();

        return CalculateCci(high, low, close, period).ToList();

        #region bing ai generated

        // //将最高价、最低价和收盘价三个数组按元素相加，并除以3，得到一个中价（TYP）数组
        // var typ = high.Zip(low, (h, l) => h + l).Zip(close, (hl, c) => hl + c).Select(x => x / 3).ToArray();
        // //计算中价数组的移动平均值和移动平均绝对偏差
        // var ma = Ma(typ, period);
        // var demavend = Avedev(ma, period);
        // //将中价数组中的每个元素减去移动平均值，除以移动平均绝对偏差，并乘以一个常数（默认为0.015），得到一个CCI数组
        // return typ.Select((x, i) => (x - ma[i]) / demavend[i] * constant).ToList();

        #endregion
    }

    private static IEnumerable<double> CalculateCci(IReadOnlyList<double> high, IReadOnlyList<double> low,
        IReadOnlyList<double> close, int period = 14)
    {
        var typicalPrice = new double[close.Count];
        for (var i = 0; i < close.Count; i++) typicalPrice[i] = (high[i] + low[i] + close[i]) / 3;

        var ma = CalculateMa(typicalPrice, period);
        var meanDeviation = new double[typicalPrice.Length];
        for (var i = period - 1; i < typicalPrice.Length; i++)
        {
            double sum = 0;
            for (var j = i - period + 1; j <= i; j++) sum += Math.Abs(typicalPrice[j] - ma[i]);
            meanDeviation[i] = sum / period;
        }

        var cci = new double[typicalPrice.Length];
        for (var i = period - 1; i < typicalPrice.Length; i++)
            cci[i] = (typicalPrice[i] - ma[i]) / (CciConstant * meanDeviation[i]);

        return cci;
    }

    private static double[] CalculateMa(IReadOnlyList<double> data, int period)
    {
        var result = new double[data.Count];
        var sum = 0.0;
        for (var i = 0; i < data.Count; i++)
        {
            sum += data[i];
            if (i >= period)
            {
                sum -= data[i - period];
                result[i] = sum / period;
            }
            else
            {
                result[i] = sum / (i + 1);
            }
        }

        return result;
    }

    #region bing ai generate

    // //计算移动平均值
    // private static double[] Ma(double[] array, int period)
    // {
    //     //创建一个结果数组，长度与输入数组相同
    //     var result = new double[array.Length];
    //     //从第period个元素开始，遍历输入数组
    //     for (var i = period - 1; i < array.Length; i++)
    //     {
    //         //计算当前元素前period个元素的和
    //         double sum = 0;
    //         for (var j = 0; j < period; j++) sum += array[i - j];
    //         //计算当前元素前period个元素的平均值，并存入结果数组
    //         result[i] = sum / period;
    //     }
    //
    //     //返回结果数组
    //     return result;
    // }
    //
    // // 计算平均绝对偏差
    // private static double[] Avedev(double[] array, int period)
    // {
    //     //创建一个结果数组，长度与输入数组相同
    //     var result = new double[array.Length];
    //     //从第period个元素开始，遍历输入数组
    //     for (var i = period - 1; i < array.Length; i++)
    //     {
    //         //计算当前元素前period个元素的平均值
    //         double average = 0;
    //         for (var j = 0; j < period; j++) average += array[i - j];
    //         average /= period;
    //         //计算当前元素前period个元素与平均值之差的绝对值的和
    //         double sumOfAbsoluteDeviations = 0;
    //         for (var j = 0; j < period; j++) sumOfAbsoluteDeviations += Math.Abs(array[i - j] - average);
    //         //计算当前元素前period个元素的平均绝对偏差，并存入结果数组
    //         result[i] = sumOfAbsoluteDeviations / period;
    //     }
    //
    //     //返回结果数组
    //     return result;
    // }

    #endregion
}