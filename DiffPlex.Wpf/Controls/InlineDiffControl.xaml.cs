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
    /// The inline diff control for text.
    /// Interaction logic for InlineDiffControl.xaml
    /// </summary>
    public partial class InlineDiffControl : UserControl
    {
        /// <summary>
        /// The property of diff model.
        /// </summary>
        public static readonly DependencyProperty DiffModelProperty =
             DependencyProperty.Register("DiffModel", typeof(DiffPaneModel),
             typeof(InlineDiffControl), new FrameworkPropertyMetadata(null, (d, e) =>
             {
                 if (!(d is InlineDiffControl c) || e.OldValue == e.NewValue) return;
                 if (e.NewValue == null)
                 {
                     c.UpdateContent(null);
                     return;
                 }

                 if (!(e.NewValue is DiffPaneModel model)) return;
                 c.UpdateContent(model);
             }));

        /// <summary>
        /// The property of line number background brush.
        /// </summary>
        public static readonly DependencyProperty LineNumberForegroundProperty = RegisterDependencyProperty<Brush>("LineNumberForeground", new SolidColorBrush(Color.FromArgb(255, 64, 128, 160)));

        /// <summary>
        /// The property of line number.
        /// </summary>
        public static readonly DependencyProperty LineNumberWidthProperty = RegisterDependencyProperty<double>("LineNumberWidth", 60);

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
        public static readonly DependencyProperty ModifiedForegroundProperty = RegisterDependencyProperty<Brush>("ModifiedForeground");

        /// <summary>
        /// The property of text inserted background brush.
        /// </summary>
        public static readonly DependencyProperty ModifiedBackgroundProperty = RegisterDependencyProperty<Brush>("ModifiedBackground", new SolidColorBrush(Color.FromArgb(64, 255, 216, 0)));

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
        /// Initializes a new instance of the InlineDiffControl class.
        /// </summary>
        public InlineDiffControl()
        {
            InitializeComponent();
            ContentPanel.SetBinding(ForegroundProperty, new Binding("Foreground") { Source = this, Mode = BindingMode.OneWay });
        }

        /// <summary>
        /// Gets or sets the side by side diff model.
        /// </summary>
        public DiffPaneModel DiffModel
        {
            get => (DiffPaneModel)GetValue(DiffModelProperty);
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
            var builder = new InlineDiffBuilder(differ);
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhitespace);
        }

        /// <summary>
        /// Sets a new diff model.
        /// </summary>
        /// <param name="builder">The differ builder instance.</param>
        /// <param name="oldText">The old text string to compare.</param>
        /// <param name="newText">The new text string.</param>
        /// <param name="ignoreWhitespace">true if ignore the white space; otherwise, false.</param>
        public void SetDiffModel(InlineDiffBuilder builder, string oldText, string newText, bool ignoreWhitespace = true)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "builder should not be null.");
            DiffModel = builder.BuildDiffModel(oldText, newText, ignoreWhitespace);
        }

        /// <summary>
        /// Updates the content.
        /// </summary>
        /// <param name="m">The diff model.</param>
        private void UpdateContent(DiffPaneModel m)
        {
            ContentPanel.Clear();
            if (m?.Lines == null) return;
            foreach (var line in m.Lines)
            {
                if (line == null)
                {
                    ContentPanel.Add(null, null, null, ChangeType.Unchanged.ToString(), this);
                    continue;
                }

                var changeType = line.Type;
                ContentPanel.Add(line.Position, changeType switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    ChangeType.Modified => "M",
                    _ => " "
                }, line.Text, changeType.ToString(), this);
            }

            ContentPanel.AdjustScrollView();
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(InlineDiffControl), null);
        }

        private static DependencyProperty RegisterDependencyProperty<T>(string name, T defaultValue, PropertyChangedCallback propertyChangedCallback = null)
        {
            return DependencyProperty.Register(name, typeof(T), typeof(InlineDiffControl), new PropertyMetadata(defaultValue, propertyChangedCallback));
        }
    }
}
