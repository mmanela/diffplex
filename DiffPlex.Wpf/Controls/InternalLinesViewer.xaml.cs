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

    public WrapPanel Add(int? number, string operation, string value, string changeType, IDiffViewer diffViewer)
    {
        return (WrapPanel)AddInternal(number, operation, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(value, changeType) }, changeType, diffViewer, true);
    }

    public WrapPanel Add(int? number, string operation, List<KeyValuePair<string, string>> value, string changeType, IDiffViewer source)
    {
        return (WrapPanel)AddInternal(number, operation, value, changeType, source, true);
    }

    public StackPanel AddNoWrap(int? number, string operation, string value, string changeType, IDiffViewer source)
    {
        return (StackPanel)AddInternal(number, operation, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(value, changeType) }, changeType, source, false);
    }

    public StackPanel AddNoWrap(int? number, string operation, List<KeyValuePair<string, string>> value, string changeType, IDiffViewer source)
    {
        return (StackPanel)AddInternal(number, operation, value, changeType, source, false);
    }

    //The Panel type is a common base class for both WrapPanel and StackPanel.
    private Panel AddInternal(int? number, string operation, List<KeyValuePair<string, string>> value, string changeType, IDiffViewer source, bool isWrap)
    {
        IsTextWrapEnabled = source.IsTextWrapEnabled;

        var index = CreateTextBlock(number.HasValue ? number.ToString() : string.Empty, "LineNumberForeground", changeType + "Background", source, TextAlignment.Right);
        NumberPanel.Children.Add(index);

        var op = CreateTextBlock(operation, "ChangeTypeForeground", changeType + "Background", source, TextAlignment.Center);
        OperationPanel.Children.Add(op);

        Panel panel = isWrap ? new WrapPanel { Orientation = Orientation.Horizontal } : new StackPanel { Orientation = Orientation.Horizontal };
        panel.SetBinding(BackgroundProperty, GetBindings(changeType + "Background", source));

        foreach (var ele in value)
        {
            if (string.IsNullOrEmpty(ele.Key)) continue;
            var text = CreateTextBlock(ele.Key, ele.Value + "Foreground", ele.Value + "Background", source, TextAlignment.Left, IsTextWrapEnabled ? TextWrapping.Wrap : TextWrapping.NoWrap);
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
        ValuePanel.CanHorizontallyScroll = !isWrap;
        ValueScrollViewer.HorizontalScrollBarVisibility = isWrap ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Visible;

        return panel;
    }

    private TextBlock CreateTextBlock(string text, string foregroundKey, string backgroundKey, IDiffViewer source, TextAlignment alignment, TextWrapping wrapping = TextWrapping.NoWrap)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            TextAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Top,
            TextWrapping = wrapping,
            Margin = new Thickness(2)
        };
        textBlock.SetBinding(TextBlock.ForegroundProperty, GetBindings(foregroundKey, source, Foreground));
        textBlock.SetBinding(TextBlock.BackgroundProperty, GetBindings(backgroundKey, source));
        ApplyTextBlockProperties(textBlock, source);
        return textBlock;
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
