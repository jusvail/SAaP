using System.Collections.Generic;
using Windows.UI;
using Microsoft.UI.Xaml.Controls;

namespace SAaP.Chart.Contracts.Services;

public interface IChartService
{
    void DrawBar(Canvas canvas, List<IList<double>> dataList, List<string> names, List<List<string>> nameInfo);
}