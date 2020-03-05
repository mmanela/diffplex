﻿using System;
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
    /// Interaction logic for InternalLinesViewer.xaml
    /// </summary>
    internal partial class InternalLinesViewer : UserControl
    {
        private readonly Dictionary<string, Binding> bindings = new Dictionary<string, Binding>();

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
            index.SetBinding(TextBlock.ForegroundProperty, GetBindings("LineNumberForeground", source, Foreground));
            index.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", source));
            ApplyTextBlockProperties(index, source);
            NumberPanel.Children.Add(index);

            var op = new TextBlock
            {
                Text = operation,
                TextAlignment = TextAlignment.Center
            };
            op.SetBinding(TextBlock.ForegroundProperty, GetBindings("ChangeTypeForeground", source, Foreground));
            op.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", source));
            ApplyTextBlockProperties(op, source);
            OperationPanel.Children.Add(op);

            var text = new TextBlock
            {
                Text = value
            };
            if (!string.IsNullOrEmpty(value))
                text.SetBinding(TextBlock.ForegroundProperty, GetBindings(changeType + "Foreground", source, Foreground));
            text.SetBinding(TextBlock.BackgroundProperty, GetBindings(changeType + "Background", source));
            ApplyTextBlockProperties(text, source);
            ValuePanel.Children.Add(text);
            return text;
        }

        private Binding GetBindings(string key, UIElement source)
        {
            if (bindings.TryGetValue(key, out var r) && r.Source == source) return r;
            return bindings[key] = new Binding(key) { Source = source, Mode = BindingMode.OneWay };
        }

        private Binding GetBindings(string key, UIElement source, object defaultValue)
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

        private void ApplyTextBlockProperties(TextBlock text, UIElement source)
        {
            text.SetBinding(TextBlock.FontSizeProperty, GetBindings("FontSize", source));
            text.SetBinding(TextBlock.FontFamilyProperty, GetBindings("FontFamily", source, Helper.FontFamily ));
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
}
