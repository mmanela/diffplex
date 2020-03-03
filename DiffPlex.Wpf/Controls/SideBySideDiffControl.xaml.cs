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

using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.Wpf.Controls
{
    /// <summary>
    /// The side by side diff control for text.
    /// Interaction logic for SideBySideDiffControl.xaml
    /// </summary>
    public partial class SideBySideDiffControl : UserControl
    {
        /// <summary>
        /// The property of diff model.
        /// </summary>
        public static readonly DependencyProperty DiffModelProperty =
             DependencyProperty.Register("DiffModel", typeof(SideBySideDiffModel),
             typeof(SideBySideDiffControl), new FrameworkPropertyMetadata(null, (d, e) =>
             {
                 if (!(d is SideBySideDiffControl c) || e.OldValue == e.NewValue) return;
                 if (e.NewValue == null)
                 {
                     c.UpdateContent(null);
                     return;
                 }

                 if (!(e.NewValue is SideBySideDiffModel model)) return;
                 c.UpdateContent(model);
             }));

        /// <summary>
        /// The property of line number background brush.
        /// </summary>
        public static readonly DependencyProperty LineNumberForegroundProperty = RegisterDependencyProperty<Brush>("LineNumberForeground", new SolidColorBrush(Color.FromArgb(255, 64, 128, 160)));

        /// <summary>
        /// The property of line number width.
        /// </summary>
        public static readonly DependencyProperty LineNumberWidthProperty = RegisterDependencyProperty<double>("LineNumberWidth", 60, (d, e) =>
        {
            if (!(d is SideBySideDiffControl c) || e.OldValue == e.NewValue || !(e.NewValue is int n)) return;
            c.LineNumberWidth = n;
        });

        /// <summary>
        /// The property of change type foreground brush.
        /// </summary>
        public static readonly DependencyProperty ChangeTypeForegroundProperty = RegisterDependencyProperty<Brush>("ChangeTypeForeground", new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)));

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty InsertedForegroundProperty = RegisterDependencyProperty<Brush>("InsertedForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty InsertedBackgroundProperty = RegisterDependencyProperty<Brush>("InsertedBackground", new SolidColorBrush(Color.FromArgb(64, 96, 216, 32)));

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty DeletedForegroundProperty = RegisterDependencyProperty<Brush>("DeletedForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty DeletedBackgroundProperty = RegisterDependencyProperty<Brush>("DeletedBackground", new SolidColorBrush(Color.FromArgb(64, 216, 32, 32)));

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty UnchangedForegroundProperty = RegisterDependencyProperty<Brush>("UnchangedForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty UnchangedBackgroundProperty = RegisterDependencyProperty<Brush>("UnchangedBackground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty ImaginaryForegroundProperty = RegisterDependencyProperty<Brush>("ImaginaryForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty ImaginaryBackgroundProperty = RegisterDependencyProperty<Brush>("ImaginaryBackground");

        /// <summary>
        /// The property of grid splitter background brush.
        /// </summary>
        public static readonly DependencyProperty SplitterForegroundProperty = RegisterDependencyProperty<Brush>("SplitterForeground");

        /// <summary>
        /// The property of grid splitter background brush.
        /// </summary>
        public static readonly DependencyProperty SplitterBackgroundProperty = RegisterDependencyProperty<Brush>("SplitterBackground", new SolidColorBrush(Color.FromArgb(64, 128, 128, 128)));

        /// <summary>
        /// The property of grid splitter border brush.
        /// </summary>
        public static readonly DependencyProperty SplitterBorderBrushProperty = RegisterDependencyProperty<Brush>("SplitterBorderBrush");

        /// <summary>
        /// The property of grid splitter border thickness.
        /// </summary>
        public static readonly DependencyProperty SplitterBorderThicknessProperty = RegisterDependencyProperty<Thickness>("SplitterBorderThickness");

        /// <summary>
        /// The property of grid splitter width.
        /// </summary>
        public static readonly DependencyProperty SplitterWidthProperty = RegisterDependencyProperty<double>("SplitterWidth", 5);

        /// <summary>
        /// Initializes a new instance of the SideBySideDiffControl class.
        /// </summary>
        public SideBySideDiffControl()
        {
            InitializeComponent();

            LeftContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LeftContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
            RightContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(ForegroundProperty, new Binding("SplitterForeground") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BackgroundProperty, new Binding("SplitterBackground") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BorderBrushProperty, new Binding("SplitterBorderBrush") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BorderThicknessProperty, new Binding("SplitterThickness") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(WidthProperty, new Binding("SplitterWidth") { Source = this, Mode = BindingMode.OneWay });
        }

        /// <summary>
        /// Gets or sets the side by side diff model.
        /// </summary>
        public SideBySideDiffModel DiffModel
        {
            get => (SideBySideDiffModel)GetValue(DiffModelProperty);
            set => SetValue(DiffModelProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line number.
        /// </summary>
        public Brush LineNumberForeground
        {
            get => (Brush)GetValue(LineNumberForegroundProperty);
            set => SetValue(LineNumberForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the line number width.
        /// </summary>
        public int LineNumberWidth
        {
            get => (int)GetValue(LineNumberWidthProperty);
            set => SetValue(LineNumberWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the change type.
        /// </summary>
        public Brush ChangeTypeForeground
        {
            get => (Brush)GetValue(ChangeTypeForegroundProperty);
            set => SetValue(ChangeTypeForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line added.
        /// </summary>
        public Brush InsertedForeground
        {
            get => (Brush)GetValue(InsertedForegroundProperty);
            set => SetValue(InsertedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line added.
        /// </summary>
        public Brush InsertedBackground
        {
            get => (Brush)GetValue(InsertedBackgroundProperty);
            set => SetValue(InsertedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line deleted.
        /// </summary>
        public Brush DeletedForeground
        {
            get => (Brush)GetValue(DeletedForegroundProperty);
            set => SetValue(DeletedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line deleted.
        /// </summary>
        public Brush DeletedBackground
        {
            get => (Brush)GetValue(DeletedBackgroundProperty);
            set => SetValue(DeletedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line unchanged.
        /// </summary>
        public Brush UnchangedForeground
        {
            get => (Brush)GetValue(UnchangedForegroundProperty);
            set => SetValue(UnchangedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line unchanged.
        /// </summary>
        public Brush UnchangedBackground
        {
            get => (Brush)GetValue(UnchangedBackgroundProperty);
            set => SetValue(UnchangedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line imaginary.
        /// </summary>
        public Brush ImaginaryForeground
        {
            get => (Brush)GetValue(ImaginaryForegroundProperty);
            set => SetValue(ImaginaryForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line imaginary.
        /// </summary>
        public Brush ImaginaryBackground
        {
            get => (Brush)GetValue(ImaginaryBackgroundProperty);
            set => SetValue(ImaginaryBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the grid splitter.
        /// </summary>
        public Brush SplitterForeground
        {
            get => (Brush)GetValue(SplitterForegroundProperty);
            set => SetValue(SplitterForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the grid splitter.
        /// </summary>
        public Brush SplitterBackground
        {
            get => (Brush)GetValue(SplitterBackgroundProperty);
            set => SetValue(SplitterBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the border brush of the grid splitter.
        /// </summary>
        public Brush SplitterBorderBrush
        {
            get => (Brush)GetValue(SplitterBackgroundProperty);
            set => SetValue(SplitterBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness of the grid splitter.
        /// </summary>
        public Thickness SplitterBorderThickness
        {
            get => (Thickness)GetValue(SplitterBorderThicknessProperty);
            set => SetValue(SplitterBorderThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness of the grid splitter.
        /// </summary>
        public double SplitterWidth
        {
            get => (double)GetValue(SplitterWidthProperty);
            set => SetValue(SplitterWidthProperty, value);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="differ">The differ instance.</param>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhitespace">true if ignore the white space; otherwise, false.</param>
        public void SetDiffModel(IDiffer differ, string oldText, string newText, bool ignoreWhitespace = true)
        {
            if (differ == null) throw new ArgumentNullException(nameof(differ), "differ should not be null.");
            var builder = new SideBySideDiffBuilder(differ);
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhitespace);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="builder">The differ builder instance.</param>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhitespace">true if ignore the white space; otherwise, false.</param>
        public void SetDiffModel(SideBySideDiffBuilder builder, string oldText, string newText, bool ignoreWhitespace = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "builder should not be null.");
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhitespace);
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="m">The diff model.</param>
        private void UpdateContent(SideBySideDiffModel m)
        {
            LeftContentPanel.Clear();
            RightContentPanel.Clear();
            if (m == null) return;
            InsertLines(LeftContentPanel, m.OldText?.Lines, true);
            InsertLines(RightContentPanel, m.NewText?.Lines);
        }

        private void InsertLines(InternalLinesControl panel, List<DiffPiece> lines, bool isOld = false)
        {
            if (lines == null || panel == null) return;
            foreach (var line in lines)
            {
                if (line == null)
                {
                    panel.Add(null, null, null, ChangeType.Unchanged.ToString(), this);
                    continue;
                }

                var changeType = line.Type;
                if (changeType == ChangeType.Modified) changeType = isOld ? ChangeType.Deleted : ChangeType.Inserted;
                panel.Add(line.Position, changeType switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    _ => " "
                }, line.Text, changeType.ToString(), this);
            }

            panel.AdjustScrollView();
        }

        //private void InsertLines(Panel panel, List<DiffPiece> lines, bool isOld = false)
        //{
        //    if (lines == null || panel == null) return;
        //    foreach (var line in lines)
        //    {
        //        var stack = new StackPanel { Orientation = Orientation.Horizontal };
        //        panel.Children.Add(stack);
        //        var lineIndex = CreateTextBlock(null, "LineNumber");
        //        lineIndex.SetBinding(WidthProperty, new Binding("LineIndexWidth") { Source = this, Mode = BindingMode.OneWay });
        //        stack.Children.Add(lineIndex);
        //        if (line == null)
        //        {
        //            panel.Children.Add(new TextBlock());
        //            continue;
        //        }

        //        if (line.Position.HasValue) lineIndex.Text = line.Position.Value.ToString();
        //        var changeType = line.Type;
        //        if (changeType == ChangeType.Modified) changeType = isOld ? ChangeType.Deleted : ChangeType.Inserted;
        //        lineIndex.Text += changeType switch
        //        {
        //            ChangeType.Inserted => '+',
        //            ChangeType.Deleted => '-',
        //            _ => ' ',
        //        };
        //        stack.SetBinding(TextBlock.BackgroundProperty, new Binding(changeType + "Background") { Source = this, Mode = BindingMode.OneWay });
        //        if (line.SubPieces == null || line.SubPieces.Count == 0)
        //        {
        //            var text = CreateTextBlock(line.Text, changeType.ToString());
        //            stack.Children.Add(text);
        //            continue;
        //        }

        //        foreach (var piece in line.SubPieces)
        //        {
        //            if (piece == null) continue;
        //            var subChangeType = piece.Type;
        //            if (subChangeType == ChangeType.Modified) subChangeType = isOld ? ChangeType.Deleted : ChangeType.Inserted;
        //            var text = CreateTextBlock(piece.Text, subChangeType.ToString());
        //            stack.Children.Add(text);
        //        }
        //    }
        //}

        private void LeftContentPanel_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = LeftContentPanel.VerticalOffset;
            if (Math.Abs(RightContentPanel.VerticalOffset - offset) > 1)
                RightContentPanel.ScrollToVerticalOffset(offset);
        }

        private void RightContentPanel_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = RightContentPanel.VerticalOffset;
            if (Math.Abs(LeftContentPanel.VerticalOffset - offset) > 1)
                LeftContentPanel.ScrollToVerticalOffset(offset);
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(SideBySideDiffControl), null);
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name, T defaultValue, PropertyChangedCallback propertyChangedCallback = null)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(SideBySideDiffControl), new PropertyMetadata(defaultValue, propertyChangedCallback));
        }
    }
}
