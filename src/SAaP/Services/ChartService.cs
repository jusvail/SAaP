using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SAaP.Contracts.Services;
using SAaP.Core.Services.Generic;
using SAaP.Models;

namespace SAaP.Services;

public class ChartService : IChartService
{
	private const double ChartP                      = 12.0;
	private const double DefaultLinesStrokeThickness = 0.8;

	private const int ChartPInt = (int)ChartP;

	private static readonly SolidColorBrush HighLightStroke     = new(Colors.WhiteSmoke);
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

	public void DrawBar(Canvas canvas, List<IList<double>> dataList, List<string> names, List<List<string>> nameInfo)
	{
		if (canvas == null) return;

		if (!dataList.Any()) return;

		while (canvas.Children.Any()) canvas.Children.RemoveAt(0);

		//calc max data height
		var maxDataHeight = dataList.Aggregate(0.0, (current, data) => data.Select(Math.Abs).Prepend(current).Max());

		var groupCount  = dataList.Count;
		var columnCount = dataList[0].Count;
		var recCount    = groupCount * columnCount;

		var canvasHeight = canvas.Height;
		var canvasWidth  = canvas.Width;

		var canvasHeightInt = (int)canvasHeight;
		var canvasWidthInt  = (int)canvasWidth;

		var sumWidth = canvasWidth - 2 * ChartP;

		var absHeight = canvasHeight / 2;

		DrawAxis(canvas, canvasHeightInt, canvasWidthInt);

		var recWidth = CalcRecCellWidth(sumWidth * 0.95, recCount, groupCount);

		var gap = recWidth / 5;

		var grid = NewNotifyGrid(out var textBlock);

		Canvas.SetZIndex(grid, 99);
		Canvas.SetZIndex(textBlock, 99);
		canvas.Children.Add(grid);

		for (var i = 0; i < dataList.Count; i++)
		{
			var data      = dataList[i];
			var leftStart = ChartP + gap + recWidth * i;

			for (var j = 0; j < data.Count; j++)
			{
				var t          = data[j];
				var dataHeight = t;

				var h = CalcRecCellHeight(absHeight, dataHeight, maxDataHeight);

				var brush = dataHeight > 0 ? DefaultColorBrushes[i] : DefaultMinusColorBrushes[i];
				var rec   = NewRectangleFrom(recWidth, h, brush);

				var i1 = i;
				var j1 = j;
				rec.PointerEntered += (sender, e) =>
				{
					var point = e.GetCurrentPoint(canvas);
					textBlock.Text = $"[{names[i1]}][{nameInfo[i1][j1]}] =>{dataHeight.ToString(CultureInfo.InvariantCulture)}%";

					Canvas.SetLeft(grid, point.Position.X);
					Canvas.SetTop(grid, point.Position.Y - ChartPInt);

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

	public void DrawStatisticsResultA(Canvas canvas, StatisticsReport report)
	{
		if (canvas == null) return;
		if (!report.StatisticsResults.Any()) return;

		while (canvas.Children.Any()) canvas.Children.RemoveAt(0);

		var recCount = report.StatisticsResults.Count;

		var canvasHeight = canvas.Height;
		var canvasWidth  = canvas.Width;

		var canvasHeightInt = (int)canvasHeight;
		var canvasWidthInt  = (int)canvasWidth;

		DrawAxis(canvas, canvasHeightInt, canvasWidthInt);

		var sumWidth = canvasWidth - 2 * ChartP;

		var absHeight = canvasHeight / 2;

		// recCount * width + (recCount + 1) * (width / 5) + ChartP * 2 = canvasWidth
		// 5 * recCount * width + (recCount + 1) * width + ChartP * 10 = canvasWidth * 5

		var recWidth = (canvasWidth * 5 - ChartP * 10) / (5 * recCount + recCount + 1);

		var gap = recWidth / 5;

		var grid = NewNotifyGrid(out var textBlock);

		Canvas.SetZIndex(grid, 99);
		Canvas.SetZIndex(textBlock, 99);
		canvas.Children.Add(grid);

		var maxRecHeight = (canvasHeightInt - ChartPInt) / 2;

		var leftStart = ChartP + gap;

		var keys = report.StatisticsResults.Keys.OrderBy(d => d).ToList();
		for (var i = 0; i< keys.Count; i++)
		{
			var pullUpBefore = keys[i];

			var list         = report.StatisticsResults[pullUpBefore];

			var allCount     = list.Count;
			var successCount = list.Count(s => s.IsSuccess);
			var failCount    = allCount - successCount;

			var successHeight = maxRecHeight * successCount / allCount;
			var failHeight    = maxRecHeight * failCount / allCount;

			var recS = NewRectangleFrom(recWidth, successHeight, DefaultColorBrushes[0]);
			var recF = NewRectangleFrom(recWidth, failHeight, DefaultMinusColorBrushes[0]);

			recS.PointerEntered += (sender, e) =>
			{
				var point = e.GetCurrentPoint(canvas);
				textBlock.Text =
					$"前期上涨{pullUpBefore}%=>达成{report.TaskDetail.ExpectedProfit}%止盈数/总匹配数：{successCount}/{allCount}，{CalculationService.Round2(100 * successCount / allCount)}%";

				Canvas.SetLeft(grid, point.Position.X);
				Canvas.SetTop(grid, point.Position.Y - ChartPInt);

				grid.Visibility = Visibility.Visible;
				var recThis = sender as Rectangle;

				if (recThis == null) return;

				recS.Stroke = HighLightStroke;
			};

			recS.PointerExited += (_, _) =>
			{
				grid.Visibility = Visibility.Collapsed;

				recS.Stroke = null;
			};

			Canvas.SetTop(recS, absHeight - successHeight);
			Canvas.SetLeft(recS, leftStart);
			canvas.Children.Add(recS);

			Canvas.SetTop(recF, absHeight);
			Canvas.SetLeft(recF, leftStart);
			canvas.Children.Add(recF);

			leftStart += recWidth + gap;
		}
	}

	private static Grid NewNotifyGrid(out TextBlock textBlock)
	{
		var grid = new Grid
		{
			Visibility = Visibility.Collapsed,
			Background = HighLightBackground
		};

		textBlock = new TextBlock
		{
			VerticalAlignment = VerticalAlignment.Center,
			TextWrapping = TextWrapping.Wrap,
			Width = 150
		};
		grid.Children.Add(textBlock);
		return grid;
	}

	private static Line NewLineFrom(int x1, int y1, int x2, int y2)
	{
		return new Line
		{
			X1              = x1,
			Y1              = y1,
			X2              = x2,
			Y2              = y2,
			StrokeThickness = DefaultLinesStrokeThickness,
			Stroke          = new SolidColorBrush(Colors.Black)
		};
	}

	private static Rectangle NewRectangleFrom(double width, double height, Brush brush)
	{
		return new Rectangle
		{
			Width  = width,
			Height = height,
			Fill   = brush
		};
	}

	private static double CalcRecCellWidth(double canvasWidth, int recCount, int groupCount = 1)
	{
		// recCount * width + (groupCount + 1) * (width / 5) + ChartP * 2 = canvasWidth
		// [ 5 * recCount + (groupCount + 1)] * width = (canvasWidth -ChartP * 2) * 5
		return (canvasWidth - ChartP * 2) * 5 / (5 * recCount + groupCount + 1);
	}

	private static double CalcRecCellHeight(double canvasHeight, double dataHeight, double maxDataHeight)
	{
		// canvasHeight / h = maxDataHeight / dataHeight
		return canvasHeight * Math.Abs(dataHeight) / maxDataHeight;
	}

	private static void DrawAxis(Canvas canvas, int canvasHeightInt, int canvasWidthInt)
	{
		var lineX = NewLineFrom(ChartPInt, canvasHeightInt / 2, canvasWidthInt - ChartPInt, canvasHeightInt / 2);
		var lineY = NewLineFrom(ChartPInt, ChartPInt, ChartPInt, canvasHeightInt - ChartPInt);

		// foreground display 
		Canvas.SetZIndex(lineX, 98);
		Canvas.SetZIndex(lineY, 98);

		canvas.Children.Add(lineX);
		canvas.Children.Add(lineY);
	}
}