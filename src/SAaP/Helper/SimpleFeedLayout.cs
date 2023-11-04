using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace SAaP.Helper;

internal class SimpleFeedLayout : VirtualizingLayout
{

    private double _rowSpacing;
    private double _colSpacing;
    private Size _minItemSize = Size.Empty;

    public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register(
        nameof(RowSpacing), typeof(double), typeof(SimpleFeedLayout), new PropertyMetadata(default(double), OnPropertyChanged));

    public double RowSpacing
    {
        get => _rowSpacing;
        set => SetValue(RowSpacingProperty, value);
    }

    public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register(
        nameof(ColumnSpacing), typeof(double), typeof(SimpleFeedLayout), new PropertyMetadata(default(double), OnPropertyChanged));

    public double ColumnSpacing
    {
        get => _colSpacing;
        set => SetValue(ColumnSpacingProperty, value);
    }

    public static readonly DependencyProperty MinItemSizeProperty = DependencyProperty.Register(
        nameof(MinItemSize), typeof(Size), typeof(SimpleFeedLayout), new PropertyMetadata(default(Size), OnPropertyChanged));

    public Size MinItemSize
    {
        get => _minItemSize;
        set => SetValue(MinItemSizeProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is not SimpleFeedLayout layout) return;

        if (args.Property == RowSpacingProperty)
        {
            layout._rowSpacing = (double)args.NewValue;
        }
        else if (args.Property == ColumnSpacingProperty)
        {
            layout._colSpacing = (double)args.NewValue;
        }
        else if (args.Property == MinItemSizeProperty)
        {
            layout._minItemSize = (Size)args.NewValue;
        }
        else
        {
            throw new InvalidOperationException("Seriously?");
        }

        layout.InvalidateMeasure();
    }

    protected override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        base.InitializeForContextCore(context);

        if (context.LayoutState is not SimpleFeedLayoutState)
        {
            context.LayoutState = new SimpleFeedLayoutState();
        }
    }

    protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        base.UninitializeForContextCore(context);

        context.LayoutState = null;
    }

    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        if (_minItemSize == Size.Empty)
        {
            var firstElement = context.GetOrCreateElementAt(0);

            firstElement.Measure(new Size(float.PositiveInfinity, float.PositiveInfinity));

            _minItemSize = firstElement.DesiredSize;
        }

        var firstRowIndex = Math.Max(
            (int)(context.RealizationRect.Y / (_minItemSize.Height + _rowSpacing)) - 1,
            0);

        var lastRowIndex = Math.Min(
            (int)(context.RealizationRect.Bottom / (_minItemSize.Height + _rowSpacing)) + 1,
            context.ItemCount / 4);

        var state = context.LayoutState as SimpleFeedLayoutState;
        state!.LayoutRects.Clear();

        state.FirstRealizedIndex = firstRowIndex * 4;

        var maxRowWidth = 0.0;

        for (var rowIndex = firstRowIndex; rowIndex <= lastRowIndex; rowIndex++)
        {
            var firstItemIndex = rowIndex * 4;

            var curRowWidth = 0.0;

            for (var columnIndex = 0; columnIndex < 4; columnIndex++)
            {
                var index = firstItemIndex + columnIndex;
                if (index >= context.ItemCount) break;

                var container = context.GetOrCreateElementAt(index);

                var size = new Size
                {
                    Height = _minItemSize.Height
                };

                if (container is not ToggleButton button) break;
                {
                    var content = button.Content;
                    var len = content.ToString()?.Length;
                    if (len != null)
                    {
                        size.Width = (double)(15 * len);
                    }
                }

                container.Measure(size);

                var rec = new Rect
                {
                    Width = size.Width,
                    Height = size.Height,
                    Y = rowIndex * (_minItemSize.Height + _rowSpacing)
                };

                if (columnIndex == 0)
                {
                    rec.X = 0;
                }
                else
                {
                    rec.X = state.LayoutRects[^1].Right + _colSpacing;
                }

                state.LayoutRects.Add(rec);
                curRowWidth += rec.Width;
            }

            if (curRowWidth > maxRowWidth)
            {
                maxRowWidth = curRowWidth;
            }
        }

        var extentHeight = Math.Max(0, (context.ItemCount / 4 - 1) * (_minItemSize.Height + _rowSpacing) + _minItemSize.Height);

        return new Size(maxRowWidth, extentHeight);
    }

    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var state = context.LayoutState as SimpleFeedLayoutState;

        var currentIndex = state!.FirstRealizedIndex;

        foreach (var arrangeRect in state.LayoutRects)
        {
            var container = context.GetOrCreateElementAt(currentIndex);
            container.Arrange(arrangeRect);
            currentIndex++;
        }

        return finalSize;
    }

}

internal class SimpleFeedLayoutState
{
    public int FirstRealizedIndex { get; set; }

    /// <summary>
    /// List of layout bounds for items starting with the
    /// FirstRealizedIndex.
    /// </summary>
    public List<Rect> LayoutRects
    {
        get
        {
            if (_layoutRects == null)
            {
                _layoutRects = new List<Rect>();
            }

            return _layoutRects;
        }
    }

    private List<Rect> _layoutRects;
}