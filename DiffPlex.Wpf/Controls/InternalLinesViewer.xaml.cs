using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiffPlex.Wpf.Controls;

/// <summary>
/// Interaction logic for InternalLinesViewer.xaml
/// </summary>
internal partial class InternalLinesViewer : UserControl
{
    private readonly Dictionary<string, Binding> bindings = new();

    public InternalLinesViewer()
    {
        InitializeComponent();
    }

    public event ScrollChangedEventHandler ScrollChanged
    {
        add => ValueScrollViewer.ScrollChanged += value;
        remove => ValueScrollViewer.ScrollChanged -= value;
    }

    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => ValueScrollViewer.VerticalScrollBarVisibility;
        set => ValueScrollViewer.VerticalScrollBarVisibility = value;
    }

    public Guid TrackingId { get; set; }

    public ContextMenu LineContextMenu { get; set; }

    public bool IsTextWrapEnabled { get; set; }

    public double VerticalOffset => ValueScrollViewer.VerticalOffset;

    public int LineNumberWidth
    {
        get
        {
            return (int)(NumberColumn.ActualWidth + OperationColumn.ActualWidth);
        }

        set
        {
            var aThird = value / 3;
            OperationColumn.Width = new GridLength(aThird);
            NumberColumn.Width = new GridLength(value - aThird);
        }
    }

    public int Count => ValuePanel.Children.Count;

    public void Clear()
    {
        NumberPanel.Children.Clear();
        OperationPanel.Children.Clear();
        ValuePanel.Children.Clear();
    }
    

    public Panel Add(int? number, string operation, string value, string changeType, IDiffViewer diffViewer)
    {       
        IsTextWrapEnabled = diffViewer.IsTextWrapEnabled;

        Panel panel = IsTextWrapEnabled ? new WrapPanel { Orientation = Orientation.Horizontal }
                                        : new StackPanel { Orientation = Orientation.Horizontal };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var index = new TextBlock
        {
            Text = number.HasValue ? number.ToString() : string.Empty,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = IsTextWrapEnabled ? VerticalAlignment.Top : VerticalAlignment.Stretch,
            Margin =  IsTextWrapEnabled ? new Thickness(2) : new Thickness(0)
        };
        index.SetBinding(TextBlock.ForegroundProperty, GetBindings("LineNumberForeground", diffViewer, Foreground));
        index.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", diffViewer));
        ApplyTextBlockProperties(index, diffViewer);
        if(!IsTextWrapEnabled)
            NumberPanel.Children.Add(index);

        var op = new TextBlock
        {
            Text = operation,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = IsTextWrapEnabled ? VerticalAlignment.Top : VerticalAlignment.Stretch,
            Margin = IsTextWrapEnabled ? new Thickness(2) : new Thickness(0)
        };
        op.SetBinding(TextBlock.ForegroundProperty, GetBindings("ChangeTypeForeground", diffViewer, Foreground));
        op.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", diffViewer));
        ApplyTextBlockProperties(op, diffViewer);
        if (!IsTextWrapEnabled)
            OperationPanel.Children.Add(op);

        if(IsTextWrapEnabled)
        {
            Grid.SetColumn(index, 0);
            Grid.SetColumn(op, 1);
            grid.Children.Add(index);
            grid.Children.Add(op);
        }

        var text = new TextBlock
        {
            Text = value,
            TextWrapping = IsTextWrapEnabled ? TextWrapping.Wrap : TextWrapping.NoWrap,
            VerticalAlignment = IsTextWrapEnabled ? VerticalAlignment.Top : VerticalAlignment.Stretch,
            Margin = IsTextWrapEnabled ? new Thickness(5, 0, 5, 0) : new Thickness(0)
        };
        if (!string.IsNullOrEmpty(value))
        {
            text.SetBinding(TextBlock.ForegroundProperty, GetBindings(changeType + "Foreground", diffViewer, Foreground));
            text.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", diffViewer));
            ApplyTextBlockProperties(text, diffViewer);
            if (IsTextWrapEnabled)
                grid.ContextMenu = LineContextMenu;
            else
                panel.ContextMenu = LineContextMenu;
        }

        if(IsTextWrapEnabled)
        {
            Grid.SetColumn(text, 2);
            grid.Children.Add(text);
        }

        if (IsTextWrapEnabled)
            panel.Children.Add(grid);
        else
            panel.Children.Add(text);

        ValuePanel.Children.Add(panel);
        ValuePanel.CanHorizontallyScroll = false;
        ValueScrollViewer.HorizontalScrollBarVisibility =  IsTextWrapEnabled ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Visible;

        return panel;
    }

    public Panel Add(int? number, string operation, List<KeyValuePair<string, string>> value, string changeType, IDiffViewer diffViewer)
    {
        IsTextWrapEnabled = diffViewer.IsTextWrapEnabled;

        var index = new TextBlock
        {
            Text = number.HasValue ? number.ToString() : string.Empty,
            TextAlignment = IsTextWrapEnabled ? TextAlignment.Center : TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        index.SetBinding(TextBlock.ForegroundProperty, GetBindings("LineNumberForeground", diffViewer, Foreground));
        index.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", diffViewer));
        ApplyTextBlockProperties(index, diffViewer);
        NumberPanel.Children.Add(index);

        var op = new TextBlock
        {
            Text = operation,
            TextAlignment = IsTextWrapEnabled ? TextAlignment.Center : TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        op.SetBinding(TextBlock.ForegroundProperty, GetBindings("ChangeTypeForeground", diffViewer, Foreground));
        op.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", diffViewer));
        ApplyTextBlockProperties(op, diffViewer);
        OperationPanel.Children.Add(op);

        Panel panel = IsTextWrapEnabled ? new WrapPanel { Orientation = Orientation.Horizontal }
                                 : new StackPanel { Orientation = Orientation.Horizontal };

        panel.SetBinding(BackgroundProperty, GetBindings(changeType + "Background", diffViewer));
        value ??= new List<KeyValuePair<string, string>>();
        foreach (var ele in value)
        {
            if (string.IsNullOrEmpty(ele.Key)) continue;
            var text = new TextBlock
            {
                Text = ele.Key,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = IsTextWrapEnabled ? TextWrapping.Wrap : TextWrapping.NoWrap
            };
            if (!string.IsNullOrEmpty(ele.Value))
            {
                if (!string.IsNullOrEmpty(ele.Key))
                    text.SetBinding(TextBlock.ForegroundProperty, GetBindings(ele.Value + "Foreground", diffViewer, Foreground));
                text.SetBinding(TextBlock.BackgroundProperty, GetBindings(ele.Value + "Background", diffViewer));
            }

            ApplyTextBlockProperties(text, diffViewer);
            panel.Children.Add(text);
        }

        if (panel.Children.Count == 0)
        {
            panel.Children.Add(new TextBlock());
        }
        else
        {
            panel.ContextMenu = LineContextMenu;
        }

        ValuePanel.Children.Add(panel);
        ValuePanel.CanHorizontallyScroll = false;
        ValueScrollViewer.HorizontalScrollBarVisibility = IsTextWrapEnabled ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Visible;

        return panel;
    }

    public void SetLineVisible(int index, bool visible)
    {
        try
        {
            var visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (NumberPanel.Children[index] is TextBlock number) number.Visibility = visibility;
            if (OperationPanel.Children[index] is TextBlock operation) operation.Visibility = visibility;
            if (ValuePanel.Children[index] is StackPanel value) value.Visibility = visibility;
        }
        catch (ArgumentOutOfRangeException)
        {
        }
    }

    public IEnumerable<object> GetTagsOfEachLine()
    {
        foreach (var item in ValuePanel.Children)
        {
            yield return item is StackPanel p ? p?.Tag : null;
        }
    }

    private Binding GetBindings(string key, IDiffViewer source)
    {
        if (bindings.TryGetValue(key, out var r) && r.Source == source) return r;
        return bindings[key] = new Binding(key) { Source = source, Mode = BindingMode.OneWay };
    }

    private Binding GetBindings(string key, IDiffViewer source, object defaultValue)
    {
        if (bindings.TryGetValue(key, out var r) && r.Source == source) return r;
        return bindings[key] = new Binding(key) { Source = source, Mode = BindingMode.OneWay, TargetNullValue = defaultValue };
    }

    public void ScrollToVerticalOffset(double offset)
    {
        ValueScrollViewer.ScrollToVerticalOffset(offset);
    }

    internal void AdjustScrollView()
    {
        var isV = ValueScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
        var hasV = ValuePanel.Margin.Bottom > 10;
        if (isV)
        {
            if (!hasV) ValuePanel.Margin = NumberPanel.Margin = OperationPanel.Margin = new Thickness(0, 0, 0, 20);
        }
        else
        {
            if (hasV) ValuePanel.Margin = NumberPanel.Margin = OperationPanel.Margin = new Thickness(0);
        }
    }

    private void ApplyTextBlockProperties(TextBlock text, IDiffViewer source)
    {
        text.SetBinding(TextBlock.FontSizeProperty, GetBindings("FontSize", source));
        text.SetBinding(TextBlock.FontFamilyProperty, GetBindings("FontFamily", source, Helper.FontFamily));
        text.SetBinding(TextBlock.FontWeightProperty, GetBindings("FontWeight", source));
        text.SetBinding(TextBlock.FontStretchProperty, GetBindings("FontStretch", source));
        text.SetBinding(TextBlock.FontStyleProperty, GetBindings("FontStyle", source));
    }

    private void NumberScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var offset = NumberScrollViewer.VerticalOffset;
        ScrollVertical(ValueScrollViewer, offset);
    }

    private void OperationScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var offset = OperationScrollViewer.VerticalOffset;
        ScrollVertical(ValueScrollViewer, offset);
    }

    private void ValueScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var offset = ValueScrollViewer.VerticalOffset;
        ScrollVertical(NumberScrollViewer, offset);
        ScrollVertical(OperationScrollViewer, offset);
    }

    private void ScrollVertical(ScrollViewer scrollViewer, double offset)
    {
        if (Math.Abs(scrollViewer.VerticalOffset - offset) > 1)
            scrollViewer.ScrollToVerticalOffset(offset);
    }

    private void ValueScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        AdjustScrollView();
    }
}
