using Microsoft.UI.Xaml.Controls;
using SAaP.Models;
using System.Collections.ObjectModel;

namespace SAaP.Contracts.Services;

public interface IChartService
{
    void DrawBar(Canvas canvas, List<IList<double>> dataList, List<string> names, List<List<string>> nameInfo);

    void DrawStatisticsResultA(Canvas canvas, StatisticsReport report);
}