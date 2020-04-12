using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// The diff control for text.
    /// Interaction logic for DiffViewer.xaml
    /// </summary>
    public partial class DiffViewer : UserControl
    {
        /// <summary>
        /// The event arguments of view mode changed.
        /// </summary>
        public class ViewModeChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the ViewModeChangedEventArgs class.
            /// </summary>
            /// <param name="isSideBySide">true if it is side-by-side view mode.</param>
            public ViewModeChangedEventArgs(bool isSideBySide)
            {
                IsSideBySideViewMode = isSideBySide;
                IsInlineViewMode = !isSideBySide;
            }

            /// <summary>
            /// Gets a value indicating whether it is side-by-side view mode.
            /// </summary>
            public bool IsSideBySideViewMode { get; }

            /// <summary>
            /// Gets a value indicating whether it is inline view mode.
            /// </summary>
            public bool IsInlineViewMode { get; }
        }

        /// <summary>
        /// The property of old text.
        /// </summary>
        public static readonly DependencyProperty OldTextProperty = RegisterRefreshDependencyProperty<string>("OldText", null);

        /// <summary>
        /// The property of new text.
        /// </summary>
        public static readonly DependencyProperty NewTextProperty = RegisterRefreshDependencyProperty<string>("NewText", null);

        /// <summary>
        /// The property of a flag to ignore white space.
        /// </summary>
        public static readonly DependencyProperty IgnoreWhiteSpaceProperty = RegisterRefreshDependencyProperty("IgnoreWhiteSpace", true);

        /// <summary>
        /// The property of a flag to ignore case.
        /// </summary>
        public static readonly DependencyProperty IgnoreCaseProperty = RegisterRefreshDependencyProperty("IgnoreCase", false);

        /// <summary>
        /// The property of line number background brush.
        /// </summary>
        public static readonly DependencyProperty LineNumberForegroundProperty = RegisterDependencyProperty<Brush>("LineNumberForeground", new SolidColorBrush(Color.FromArgb(255, 64, 128, 160)));

        /// <summary>
        /// The property of line number width.
        /// </summary>
        public static readonly DependencyProperty LineNumberWidthProperty = RegisterDependencyProperty<double>("LineNumberWidth", 60, (d, e) =>
        {
            if (!(d is DiffViewer c) || e.OldValue == e.NewValue || !(e.NewValue is int n)) return;
            c.LeftContentPanel.LineNumberWidth = c.RightContentPanel.LineNumberWidth = c.InlineContentPanel.LineNumberWidth = n;
        });

        /// <summary>
        /// The property of change type symbol foreground brush.
        /// </summary>
        public static readonly DependencyProperty ChangeTypeForegroundProperty = RegisterDependencyProperty<Brush>("ChangeTypeForeground", new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)));

        /// <summary>
        /// The property of old text header.
        /// </summary>
        public static readonly DependencyProperty OldTextHeaderProperty = RegisterDependencyProperty<string>("OldTextHeader", null, (d, e) =>
        {
            if (!(d is DiffViewer c) || e.OldValue == e.NewValue) return;
            c.UpdateHeaderText();
        });

        /// <summary>
        /// The property of new text header.
        /// </summary>
        public static readonly DependencyProperty NewTextHeaderProperty = RegisterDependencyProperty<string>("NewTextHeader", null, (d, e) =>
        {
            if (!(d is DiffViewer c) || e.OldValue == e.NewValue) return;
            c.UpdateHeaderText();
        });

        /// <summary>
        /// The property of header height.
        /// </summary>
        public static readonly DependencyProperty HeaderHeightProperty = RegisterDependencyProperty<double>("HeaderHeight", 0, (d, e) =>
        {
            if (!(d is DiffViewer c) || e.OldValue == e.NewValue || !(e.NewValue is double n)) return;
            c.HeaderRow.Height = new GridLength(n);
            c.isHeaderEnabled = true;
        });

        /// <summary>
        /// The property of header background brush.
        /// </summary>
        public static readonly DependencyProperty HeaderForegroundProperty = RegisterDependencyProperty<Brush>("HeaderForeground");

        /// <summary>
        /// The property of header background brush.
        /// </summary>
        public static readonly DependencyProperty HeaderBackgroundProperty = RegisterDependencyProperty<Brush>("HeaderBackground", new SolidColorBrush(Color.FromArgb(12, 128, 128, 128)));

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
        public static readonly DependencyProperty ImaginaryBackgroundProperty = RegisterDependencyProperty<Brush>("ImaginaryBackground", new SolidColorBrush(Color.FromArgb(24, 128, 128, 128)));

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
        /// The property of IsSideBySide.
        /// </summary>
        public static readonly DependencyProperty IsSideBySideProperty = RegisterDependencyProperty<bool>(nameof(IsSideBySide), true, (d, e) =>
        {
            if (!(d is DiffViewer c) || e.OldValue == e.NewValue || !(e.NewValue is bool b)) return;
            c.ChangeViewMode(b);
        });

        /// <summary>
        /// The side-by-side diffs result.
        /// </summary>
        private SideBySideDiffModel sideBySideResult;

        /// <summary>
        /// The inline diffs result.
        /// </summary>
        private DiffPaneModel inlineResult;

        /// <summary>
        /// The flag to enable header.
        /// </summary>
        private bool isHeaderEnabled;

        /// <summary>
        /// Initializes a new instance of the DiffViewer class.
        /// </summary>
        public DiffViewer()
        {
            InitializeComponent();

            LeftContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LeftContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
            RightContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
            InlineContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BackgroundProperty, new Binding("SplitterBackground") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BorderBrushProperty, new Binding("SplitterBorderBrush") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BorderThicknessProperty, new Binding("SplitterBorderThickness") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(WidthProperty, new Binding("SplitterWidth") { Source = this, Mode = BindingMode.OneWay });
            HeaderBorder.SetBinding(BackgroundProperty, new Binding("HeaderBackground") { Source = this, Mode = BindingMode.OneWay });
            ApplyHeaderTextProperties(LeftHeaderText);
            ApplyHeaderTextProperties(RightHeaderText);
            ApplyHeaderTextProperties(InlineHeaderText);
        }

        /// <summary>
        /// Occurs when the view mode is changed.
        /// </summary>
        public event EventHandler<ViewModeChangedEventArgs> ViewModeChanged;

        /// <summary>
        /// Occurs when the grid splitter loses mouse capture.
        /// </summary>
        [Category("Behavior")]
        public event DragCompletedEventHandler SplitterDragCompleted
        {
            add => Splitter.DragCompleted += value;
            remove => Splitter.DragCompleted -= value;
        }

        /// <summary>
        /// Occurs one or more times as the mouse changes position when the grid splitter has logical focus and mouse capture.
        /// </summary>
        [Category("Behavior")]
        public event DragDeltaEventHandler SplitterDragDelta
        {
            add => Splitter.DragDelta += value;
            remove => Splitter.DragDelta -= value;
        }

        /// <summary>
        /// Occurs when the grid splitter receives logical focus and mouse capture.
        /// </summary>
        [Category("Behavior")]
        public event DragStartedEventHandler SplitterDragStarted
        {
            add => Splitter.DragStarted += value;
            remove => Splitter.DragStarted -= value;
        }

        /// <summary>
        /// Gets or sets the old text.
        /// </summary>
        [Category("Appearance")]
        public string OldText
        {
            get => (string)GetValue(OldTextProperty);
            set => SetValue(OldTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the new text.
        /// </summary>
        [Category("Appearance")]
        public string NewText
        {
            get => (string)GetValue(NewTextProperty);
            set => SetValue(NewTextProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether ignore the white space.
        /// </summary>
        [Category("Appearance")]
        public bool IgnoreWhiteSpace
        {
            get => (bool)GetValue(IgnoreWhiteSpaceProperty);
            set => SetValue(IgnoreWhiteSpaceProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether ignore case.
        /// </summary>
        [Category("Appearance")]
        public bool IgnoreCase
        {
            get => (bool)GetValue(IgnoreCaseProperty);
            set => SetValue(IgnoreCaseProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line number.
        /// </summary>
        [Bindable(true)]
        public Brush LineNumberForeground
        {
            get => (Brush)GetValue(LineNumberForegroundProperty);
            set => SetValue(LineNumberForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the line number width.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public int LineNumberWidth
        {
            get => (int)GetValue(LineNumberWidthProperty);
            set => SetValue(LineNumberWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the change type symbol.
        /// </summary>
        [Bindable(true)]
        public Brush ChangeTypeForeground
        {
            get => (Brush)GetValue(ChangeTypeForegroundProperty);
            set => SetValue(ChangeTypeForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the header of the old text.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public string OldTextHeader
        {
            get => (string)GetValue(OldTextHeaderProperty);
            set => SetValue(OldTextHeaderProperty, value);
        }

        /// <summary>
        /// Gets or sets the header of the new text.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public string NewTextHeader
        {
            get => (string)GetValue(NewTextHeaderProperty);
            set => SetValue(NewTextHeaderProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line added.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line added.
        /// </summary>
        [Bindable(true)]
        public Brush HeaderForeground
        {
            get => (Brush)GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line added.
        /// </summary>
        [Bindable(true)]
        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line added.
        /// </summary>
        [Bindable(true)]
        public Brush InsertedForeground
        {
            get => (Brush)GetValue(InsertedForegroundProperty);
            set => SetValue(InsertedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line added.
        /// </summary>
        [Bindable(true)]
        public Brush InsertedBackground
        {
            get => (Brush)GetValue(InsertedBackgroundProperty);
            set => SetValue(InsertedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line deleted.
        /// </summary>
        [Bindable(true)]
        public Brush DeletedForeground
        {
            get => (Brush)GetValue(DeletedForegroundProperty);
            set => SetValue(DeletedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line deleted.
        /// </summary>
        [Bindable(true)]
        public Brush DeletedBackground
        {
            get => (Brush)GetValue(DeletedBackgroundProperty);
            set => SetValue(DeletedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground brush of the line unchanged.
        /// </summary>
        [Bindable(true)]
        public Brush UnchangedForeground
        {
            get => (Brush)GetValue(UnchangedForegroundProperty);
            set => SetValue(UnchangedForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line unchanged.
        /// </summary>
        [Bindable(true)]
        public Brush UnchangedBackground
        {
            get => (Brush)GetValue(UnchangedBackgroundProperty);
            set => SetValue(UnchangedBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the line imaginary.
        /// </summary>
        [Bindable(true)]
        public Brush ImaginaryBackground
        {
            get => (Brush)GetValue(ImaginaryBackgroundProperty);
            set => SetValue(ImaginaryBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush of the grid splitter.
        /// </summary>
        [Bindable(true)]
        public Brush SplitterBackground
        {
            get => (Brush)GetValue(SplitterBackgroundProperty);
            set => SetValue(SplitterBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the border brush of the grid splitter.
        /// </summary>
        [Bindable(true)]
        public Brush SplitterBorderBrush
        {
            get => (Brush)GetValue(SplitterBackgroundProperty);
            set => SetValue(SplitterBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness of the grid splitter.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public Thickness SplitterBorderThickness
        {
            get => (Thickness)GetValue(SplitterBorderThicknessProperty);
            set => SetValue(SplitterBorderThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the grid splitter.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public double SplitterWidth
        {
            get => (double)GetValue(SplitterWidthProperty);
            set => SetValue(SplitterWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the IsSideBySide.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public bool IsSideBySide
        {
            get => (bool)GetValue(IsSideBySideProperty);
            set => SetValue(IsSideBySideProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the grid splitter has logical focus and mouse capture and the left mouse button is pressed.
        /// </summary>
        public bool IsSplitterDragging => Splitter.IsDragging;

        /// <summary>
        /// Gets a value that represents the actual calculated width of the left side panel.
        /// </summary>
        public double LeftSideActualWidth => LeftColumn.ActualWidth;

        /// <summary>
        /// Gets a value that represents the actual calculated width of the right side panel.
        /// </summary>
        public double RightSideActualWidth => RightColumn.ActualWidth;

        /// <summary>
        /// Gets a value indicating whether it is side-by-side view mode.
        /// </summary>
        public bool IsSideBySideViewMode => InlineContentPanel.Visibility != Visibility.Visible;

        /// <summary>
        /// Gets a value indicating whether it is inline view mode.
        /// </summary>
        public bool IsInlineViewMode => InlineContentPanel.Visibility == Visibility.Visible;

        /// <summary>
        /// Gets the side-by-side diffs result.
        /// </summary>
        public SideBySideDiffModel GetSideBySideDiffModel()
        {
            if (sideBySideResult != null || OldText == null || NewText == null) return sideBySideResult;
            sideBySideResult = SideBySideDiffBuilder.Diff(OldText, NewText, IgnoreWhiteSpace, IgnoreCase);
            RenderSideBySideDiffs();
            return sideBySideResult;
        }

        /// <summary>
        /// Gets the inline diffs result.
        /// </summary>
        public DiffPaneModel GetInlineDiffModel()
        {
            if (inlineResult != null || OldText == null || NewText == null) return inlineResult;
            inlineResult = InlineDiffBuilder.Diff(OldText, NewText, IgnoreWhiteSpace, IgnoreCase);
            RenderInlineDiffs();
            return inlineResult;
        }

        /// <summary>
        /// Refreshes.
        /// </summary>
        public void Refresh()
        {
            if (InlineContentPanel.Visibility == Visibility.Visible)
            {
                sideBySideResult = null;
                RenderSideBySideDiffs();
                if (NewText == null || OldText == null)
                {
                    inlineResult = null;
                    RenderInlineDiffs();
                    return;
                }

                inlineResult = InlineDiffBuilder.Diff(OldText, NewText, IgnoreWhiteSpace, IgnoreCase);
                RenderInlineDiffs();
                return;
            }

            inlineResult = null;
            RenderInlineDiffs();
            if (NewText == null || OldText == null)
            {
                sideBySideResult = null;
                RenderSideBySideDiffs();
                return;
            }

            sideBySideResult = SideBySideDiffBuilder.Diff(OldText, NewText, IgnoreWhiteSpace, IgnoreCase);
            RenderSideBySideDiffs();
        }

        /// <summary>
        /// Switches to the view of side-by-side diff mode.
        /// </summary>
        public void ShowSideBySide()
        {
            IsSideBySide = true;
        }

        /// <summary>
        /// Switches to the view of inline diff mode.
        /// </summary>
        public void ShowInline()
        {
            IsSideBySide = false;
        }

        private void ChangeViewMode(bool isSideBySide)
        {
            InlineContentPanel.Visibility = InlineHeaderText.Visibility
                = (isSideBySide ? Visibility.Collapsed : Visibility.Visible);
            LeftContentPanel.Visibility = RightContentPanel.Visibility = LeftHeaderText.Visibility = RightHeaderText.Visibility = Splitter.Visibility
                = (isSideBySide ? Visibility.Visible : Visibility.Collapsed);

            if (isSideBySide)
                GetSideBySideDiffModel();
            else
                GetInlineDiffModel();

            ViewModeChanged?.Invoke(this, new ViewModeChangedEventArgs(isSideBySide));
        }

        /// <summary>
        /// Goes to a specific line.
        /// </summary>
        /// <param name="lineIndex">The index of line.</param>
        /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
        /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
        public bool GoTo(int lineIndex, bool isLeftLine = false)
        {
            if (IsSideBySideViewMode) return Helper.GoTo(isLeftLine ? LeftContentPanel : RightContentPanel, lineIndex);
            else return Helper.GoTo(InlineContentPanel, lineIndex);
        }

        /// <summary>
        /// Updates the side-by-side diffs view.
        /// </summary>
        private void RenderSideBySideDiffs()
        {
            LeftContentPanel.Clear();
            RightContentPanel.Clear();
            var m = sideBySideResult;
            if (m == null) return;
            Helper.InsertLines(LeftContentPanel, m.OldText?.Lines, true, this);
            Helper.InsertLines(RightContentPanel, m.NewText?.Lines, false, this);
        }

        /// <summary>
        /// Updates the inline diffs view.
        /// </summary>
        private void RenderInlineDiffs()
        {
            Helper.RenderInlineDiffs(InlineContentPanel, inlineResult, this);
        }

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

        private void ApplyHeaderTextProperties(TextBlock text)
        {
            text.SetBinding(TextBlock.FontSizeProperty, new Binding("FontSize") { Source = this, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.FontFamilyProperty, new Binding("FontFamily") { Source = this, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.FontWeightProperty, new Binding("FontWeight") { Source = this, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.FontStretchProperty, new Binding("FontStretch") { Source = this, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.FontStyleProperty, new Binding("FontStyle") { Source = this, Mode = BindingMode.OneWay });
            text.SetBinding(TextBlock.ForegroundProperty, new Binding("HeaderForeground") { Source = this, Mode = BindingMode.OneWay, TargetNullValue = Foreground });
        }

        private void UpdateHeaderText()
        {
            LeftHeaderText.Text = OldTextHeader;
            RightHeaderText.Text = NewTextHeader;
            if (string.IsNullOrEmpty(OldTextHeader) && string.IsNullOrEmpty(NewTextHeader))
            {
                InlineHeaderText.Text = null;
                return;
            }

            InlineHeaderText.Text = $"{OldTextHeader ?? string.Empty} → {NewTextHeader ?? string.Empty}";
            if (isHeaderEnabled) return;
            HeaderHeight = 20;
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(DiffViewer), null);
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name, T defaultValue, PropertyChangedCallback propertyChangedCallback = null)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(DiffViewer), new PropertyMetadata(defaultValue, propertyChangedCallback));
        }

        private static DependencyProperty RegisterRefreshDependencyProperty<T>(string name, T defaultValue)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(DiffViewer), new PropertyMetadata(defaultValue, (d, e) =>
            {
                if (!(d is DiffViewer c) || e.OldValue == e.NewValue) return;
                c.Refresh();
            }));
        }
    }
}
