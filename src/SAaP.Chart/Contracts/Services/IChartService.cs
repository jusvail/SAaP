using System.Collections.Generic;
using Windows.UI;
using Microsoft.UI.Xaml.Controls;

namespace SAaP.Chart.Contracts.Services;

public interface IChartService
{
    void DrawBar(Canvas canvas, IList<IList<double>> dataList);
}