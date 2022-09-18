using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SAaP.Chart.Contracts.Services;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace SAaP.Chart.Services;

public class ChartService : IChartService
{
    private const double ChartP = 12.0;
    private const int DefaultLinesStrokeThickness = 2;

    private static readonly List<SolidColorBrush> DefaultColorBrushes = new()
    {
        new SolidColorBrush(Colors.DarkRed),
        new SolidColorBrush(Colors.OrangeRed),
        new SolidColorBrush(Colors.Red),
        new SolidColorBrush(Colors.MediumVioletRed),
        new SolidColorBrush(Colors.PaleVioletRed),
        new SolidColorBrush(Colors.IndianRed)
    };

    private static readonly List<SolidColorBrush> DefaultMinusColorBrushes = new()
    {
        new SolidColorBrush(Colors.DarkGreen),
        new SolidColorBrush(Colors.DarkSeaGreen),
        new SolidColorBrush(Colors.Green),
        new SolidColorBrush(Colors.DarkOliveGreen),
        new SolidColorBrush(Colors.GreenYellow),
        new SolidColorBrush(Colors.ForestGreen)
    };

    private static Line NewLineFrom(int x1, int y1, int x2, int y2)
    {
        return new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            StrokeThickness = DefaultLinesStrokeThickness,
            Stroke = new SolidColorBrush(Colors.Black)
        };
    }

    private static Rectangle NewRectangleFrom(double width, double height, SolidColorBrush brush)
    {
        return new Rectangle
        {
            Width = width,
            Height = height,
            Fill = brush
        };
    }

    private static double CalcRecCellWidth(double canvasWidth, int recCount, int groupCount)
    {
        // recCount * width + (groupCount + 1) * (width / 5) + ChartP * 2 = canvasWidth
        // [ 5 * recCount + (groupCount + 1)] * width = (canvasWidth -ChartP * 2) * 5
        return (canvasWidth - ChartP * 2) * 5 / (5 * recCount + (groupCount + 1));
    }

    private static double CalcRecCellHeight(double canvasHeight, double dataHeight)
    {
        // canvasHeight / h = 10 / dh
        return canvasHeight * Math.Abs(dataHeight) / 10;
    }

    public void DrawBar(Canvas canvas, IList<IList<double>> dataList)
    {

        if (canvas == null) return;

        if (!dataList.Any()) return;

        while (canvas.Children.Any())
        {
            canvas.Children.RemoveAt(0);
        }

        var groupCount = dataList.Count;
        var columnCount = dataList[0].Count;
        var recCount = groupCount * columnCount;

        var canvasHeight = canvas.Height;
        var canvasWidth = canvas.Width;

        var canvasHeightInt = (int)canvasHeight;
        var canvasWidthInt = (int)canvasWidth;

        var sumWidth = canvasWidth - 2 * ChartP;

        var absHeight = canvasHeight / 2;

        const int chartPInt = (int)ChartP;

        var lineX = NewLineFrom(chartPInt, canvasHeightInt / 2, canvasWidthInt - chartPInt, canvasHeightInt / 2);
        var lineY = NewLineFrom(chartPInt, chartPInt, chartPInt, canvasHeightInt - chartPInt);

        // foreground display 
        Canvas.SetZIndex(lineX,99);
        Canvas.SetZIndex(lineY,99);

        canvas.Children.Add(lineX);
        canvas.Children.Add(lineY);

        var recWidth = CalcRecCellWidth(sumWidth * 0.95, recCount, groupCount);

        var gap = recWidth / 5;

        for (var i = 0; i < dataList.Count; i++)
        {
            var data = dataList[i];
            var leftStart = ChartP + gap + recWidth * i;

            foreach (var t in data)
            {
                var dataHeight = t;
                if (dataHeight > 10) dataHeight = 9.99;
                var h = CalcRecCellHeight(absHeight, dataHeight);

                var brush = dataHeight > 0 ? DefaultColorBrushes[i] : DefaultMinusColorBrushes[i];
                var rec = NewRectangleFrom(recWidth, h, brush);

                Canvas.SetTop(rec, dataHeight > 0 ? absHeight - h : absHeight);
                Canvas.SetLeft(rec, leftStart);

                canvas.Children.Add(rec);
                leftStart += recWidth * groupCount + gap;
            }
        }
    }
}