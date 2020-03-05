﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text;
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
    /// The side by side diff control for text.
    /// Interaction logic for SideBySideDiffViewer.xaml
    /// </summary>
    public partial class SideBySideDiffViewer : UserControl
    {
        /// <summary>
        /// The property of diff model.
        /// </summary>
        public static readonly DependencyProperty DiffModelProperty =
             DependencyProperty.Register("DiffModel", typeof(SideBySideDiffModel),
             typeof(SideBySideDiffViewer), new FrameworkPropertyMetadata(null, (d, e) =>
             {
                 if (!(d is SideBySideDiffViewer c) || e.OldValue == e.NewValue) return;
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
            if (!(d is SideBySideDiffViewer c) || e.OldValue == e.NewValue || !(e.NewValue is int n)) return;
            c.LineNumberWidth = n;
        });

        /// <summary>
        /// The property of change type symbol foreground brush.
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
        /// Initializes a new instance of the SideBySideDiffViewer class.
        /// </summary>
        public SideBySideDiffViewer()
        {
            InitializeComponent();

            LeftContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            LeftContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
            RightContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BackgroundProperty, new Binding("SplitterBackground") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BorderBrushProperty, new Binding("SplitterBorderBrush") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(BorderThicknessProperty, new Binding("SplitterBorderThickness") { Source = this, Mode = BindingMode.OneWay });
            Splitter.SetBinding(WidthProperty, new Binding("SplitterWidth") { Source = this, Mode = BindingMode.OneWay });
        }

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
        /// Gets or sets the side by side diff model.
        /// </summary>
        [Category("Appearance")]
        public SideBySideDiffModel DiffModel
        {
            get => (SideBySideDiffModel)GetValue(DiffModelProperty);
            set => SetValue(DiffModelProperty, value);
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
        /// Sets a new diff model.
        /// </summary>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        public void SetDiffModel(string oldText, string newText, bool ignoreWhiteSpace = true)
        {
            var builder = new SideBySideDiffBuilder(Helper.Instance);
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="differ">The differ instance.</param>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        public void SetDiffModel(IDiffer differ, string oldText, string newText, bool ignoreWhiteSpace = true)
        {
            var builder = new SideBySideDiffBuilder(differ ?? Helper.Instance);
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="builder">The differ builder instance.</param>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <exception cref="ArgumentNullException">builder was null.</exception>
        public void SetDiffModel(SideBySideDiffBuilder builder, string oldText, string newText, bool ignoreWhiteSpace = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "builder should not be null.");
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="oldFile">The old text file to compare.</param>
        /// <param name="newFile">The new text file.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <exception cref="ArgumentNullException">oldFile or newFile was null.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="IOException">Read file failed because of I/O exception.</exception>
        /// <exception cref="UnauthorizedAccessException">Cannot access the file.</exception>
        public void SetDiffModel(FileInfo oldFile, FileInfo newFile, bool ignoreWhiteSpace = true)
        {
            if (oldFile == null) throw new ArgumentNullException(nameof(oldFile), "oldFile should not be null.");
            if (newFile == null) throw new ArgumentNullException(nameof(newFile), "newFile should not be null.");
            var oldText = File.ReadAllText(oldFile.FullName);
            var newText = File.ReadAllText(newFile.FullName);
            var builder = new SideBySideDiffBuilder(Helper.Instance);
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="oldFile">The old text file to compare.</param>
        /// <param name="newFile">The new text file.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; otherwise, false.</param>
        /// <exception cref="ArgumentNullException">oldFile or newFile was null.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="IOException">Read file failed because of I/O exception.</exception>
        /// <exception cref="UnauthorizedAccessException">Cannot access the file.</exception>
        public void SetDiffModel(FileInfo oldFile, FileInfo newFile, Encoding encoding, bool ignoreWhiteSpace = true)
        {
            if (oldFile == null) throw new ArgumentNullException(nameof(oldFile), "oldFile should not be null.");
            if (newFile == null) throw new ArgumentNullException(nameof(newFile), "newFile should not be null.");
            var oldText = File.ReadAllText(oldFile.FullName, encoding);
            var newText = File.ReadAllText(newFile.FullName, encoding);
            var builder = new SideBySideDiffBuilder(Helper.Instance);
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhiteSpace);
        }

        /// <summary>
        /// Refreshes.
        /// </summary>
        public void Refresh()
        {
            UpdateContent(DiffModel);
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

        private void InsertLines(InternalLinesViewer panel, List<DiffPiece> lines, bool isOld = false)
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
                var text = line.Text;
                switch (line.Type)
                {
                    case ChangeType.Modified:
                        changeType = isOld ? ChangeType.Deleted : ChangeType.Inserted;
                        break;
                    case ChangeType.Inserted:
                    case ChangeType.Deleted:
                    case ChangeType.Unchanged:
                        break;
                    default:
                        changeType = ChangeType.Imaginary;
                        text = string.Empty;
                        break;
                }

                panel.Add(line.Position, changeType switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    _ => " "
                }, text, changeType.ToString(), this);
            }

            panel.AdjustScrollView();
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

        private static DependencyProperty RegisterDependencyProperty<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(SideBySideDiffViewer), null);
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name, T defaultValue, PropertyChangedCallback propertyChangedCallback = null)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(SideBySideDiffViewer), new PropertyMetadata(defaultValue, propertyChangedCallback));
        }
    }
}
