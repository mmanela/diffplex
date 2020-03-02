using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiffPlex.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for SideBySideDiffControl.xaml
    /// </summary>
    internal partial class InternalLinesControl : UserControl
    {
        public InternalLinesControl()
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

        public void Clear()
        {
            NumberPanel.Children.Clear();
            OperationPanel.Children.Clear();
            ValuePanel.Children.Clear();
        }

        public TextBlock Add(int? number, string operation, string value, string changeType, UIElement source)
        {
            var index = new TextBlock
            {
                Text = number.HasValue ? number.ToString() : string.Empty,
                TextAlignment = TextAlignment.Right
            };
            index.SetBinding(TextBlock.ForegroundProperty, new Binding("LineNumberForeground") { Source = source, Mode = BindingMode.OneWay, TargetNullValue = Foreground });
            index.SetBinding(TextBlock.BackgroundProperty, new Binding(changeType + "Background") { Source = source, Mode = BindingMode.OneWay });
            ApplyTextBlockProperties(index, source);
            NumberPanel.Children.Add(index);

            var op = new TextBlock
            {
                Text = operation,
                TextAlignment = TextAlignment.Center
            };
            op.SetBinding(TextBlock.ForegroundProperty, new Binding("ChangeTypeForeground") { Source = source, Mode = BindingMode.OneWay, TargetNullValue = Foreground });
            op.SetBinding(TextBlock.BackgroundProperty, new Binding(changeType + "Background") { Source = source, Mode = BindingMode.OneWay });
            ApplyTextBlockProperties(op, source);
            OperationPanel.Children.Add(op);

            var text = new TextBlock
            {
                Text = value
            };
            text.SetBinding(TextBlock.ForegroundProperty, new Binding(changeType + "Foreground") { Source = source, Mode = BindingMode.OneWay, TargetNullValue = Foreground });
            text.SetBinding(TextBlock.BackgroundProperty, new Binding(changeType + "Background") { Source = source, Mode = BindingMode.OneWay });
            ApplyTextBlockProperties(text, source);
            ValuePanel.Children.Add(text);
            return text;
        }

        public void ScrollToVerticalOffset(double offset)
        {
            ValueScrollViewer.ScrollToVerticalOffset(offset);
        }

        private void ApplyTextBlockProperties(TextBlock text, UIElement source)
        {
            text.SetBinding(TextBlock.FontSizeProperty, new Binding("FontSize") { Source = source, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.FontFamilyProperty, new Binding("FontFamily") { Source = source, Mode = BindingMode.OneWay, TargetNullValue = "Cascadia Code, Consolas, Courier New, monospace, Microsoft Yahei, Segoe UI Emoji, Segoe UI Symbol" });
            text.SetBinding(TextBlock.FontWeightProperty, new Binding("FontWeight") { Source = source, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.FontStretchProperty, new Binding("FontStretch") { Source = source, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.FontStyleProperty, new Binding("FontStyle") { Source = source, Mode = BindingMode.OneWay });
        }

        private void NumberScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = NumberScrollViewer.VerticalOffset;
            ScrollVertical(OperationScrollViewer, offset);
            ScrollVertical(ValueScrollViewer, offset);
        }

        private void OperationScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = OperationScrollViewer.VerticalOffset;
            ScrollVertical(NumberScrollViewer, offset);
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
    }
}
