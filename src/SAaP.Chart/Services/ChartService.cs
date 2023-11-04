using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SAaP.Chart.Contracts.Services;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace SAaP.Chart.Services;

public class ChartService : IChartService
{
    private const double ChartP = 12.0;
    private const double DefaultLinesStrokeThickness = 0.8;

    private static readonly SolidColorBrush HighLightStroke = new(Colors.WhiteSmoke);
    private static readonly SolidColorBrush HighLightBackground = new(Colors.WhiteSmoke);

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

    private static Rectangle NewRectangleFrom(double width, double height, Brush brush)
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

    private static double CalcRecCellHeight(double canvasHeight, double dataHeight, double maxDataHeight)
    {
        // canvasHeight / h = maxDataHeight / dataHeight
        return canvasHeight * Math.Abs(dataHeight) / maxDataHeight;
    }

    public void DrawBar(Canvas canvas, List<IList<double>> dataList, List<string> names, List<List<string>> nameInfo)
    {
        if (canvas == null) return;

        if (!dataList.Any()) return;

        while (canvas.Children.Any())
        {
            canvas.Children.RemoveAt(0);
        }

        //calc max data height
        var maxDataHeight = dataList.Aggregate(0.0, (current, data) => data.Select(Math.Abs).Prepend(current).Max());

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
        Canvas.SetZIndex(lineX, 98);
        Canvas.SetZIndex(lineY, 98);

        canvas.Children.Add(lineX);
        canvas.Children.Add(lineY);

        var recWidth = CalcRecCellWidth(sumWidth * 0.95, recCount, groupCount);

        var gap = recWidth / 5;

        var grid = new Grid
        {
            Visibility = Visibility.Collapsed,
            Background = HighLightBackground
        };

        var textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        grid.Children.Add(textBlock);

        Canvas.SetZIndex(grid, 99);
        Canvas.SetZIndex(textBlock, 99);
        canvas.Children.Add(grid);

        for (var i = 0; i < dataList.Count; i++)
        {
            var data = dataList[i];
            var leftStart = ChartP + gap + recWidth * i;

            for (var j = 0; j < data.Count; j++)
            {
                var t = data[j];
                var dataHeight = t;

                var h = CalcRecCellHeight(absHeight, dataHeight, maxDataHeight);

                var brush = dataHeight > 0 ? DefaultColorBrushes[i] : DefaultMinusColorBrushes[i];
                var rec = NewRectangleFrom(recWidth, h, brush);

                var i1 = i;
                var j1 = j;
                rec.PointerEntered += (sender, e) =>
                {
                    var point = e.GetCurrentPoint(canvas);
                    textBlock.Text = $"[{names[i1]}][{nameInfo[i1][j1]}] =>{dataHeight.ToString(CultureInfo.InvariantCulture)}%";

                    Canvas.SetLeft(grid, point.Position.X);
                    Canvas.SetTop(grid, point.Position.Y - chartPInt);

                    grid.Visibility = Visibility.Visible;
                    var recThis = sender as Rectangle;

                    if (recThis == null) return;

                    rec.Stroke = HighLightStroke;
                };

                rec.PointerExited += (_, _) =>
                {
                    grid.Visibility = Visibility.Collapsed;

                    rec.Stroke = null;
                };

                Canvas.SetTop(rec, dataHeight > 0 ? absHeight - h : absHeight);
                Canvas.SetLeft(rec, leftStart);

                canvas.Children.Add(rec);
                leftStart += recWidth * groupCount + gap;
            }
        }
    }

    public void DrawStatisticsResultA(Canvas canvas, Dictionary<double, ObservableCollection<StatisticsResult>> dictionary)
    {

    }
}