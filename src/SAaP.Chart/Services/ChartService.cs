using System.Collections.Generic;
using Windows.UI;
using Microsoft.UI.Xaml.Controls;
using SAaP.Chart.Contracts.Services;

namespace SAaP.Chart.Services;

public class ChartService : IChartService
{
    public void DrawBar(Canvas canvas, List<List<double>> dataList, List<Color> assignedColors = null)
    {

        if (canvas == null) return;

        foreach (var data in dataList)
        {

        }
    }
}