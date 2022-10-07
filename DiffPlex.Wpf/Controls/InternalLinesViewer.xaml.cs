using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public event ScrollChangedEventHandler ScrollChanged;

        //TODO: Switch to dependency property and feed forward into the specific templated scroll control
        // We can't get the scroll viewer right after creation, so we need to bind it and let WPF handle it
        private ScrollBarVisibility verticalScrollBarVisibility;
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => verticalScrollBarVisibility;
            set => verticalScrollBarVisibility = value;
        }


        public Guid TrackingId { get; set; }

        public ContextMenu LineContextMenu { get; set; }

        public double VerticalOffset => _ValueScrollViewer.VerticalOffset;

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

        private ScrollViewer numberScrollViewer;
        internal ScrollViewer _NumberScrollViewer
        {
            get
            {
                if (numberScrollViewer == null)
                    numberScrollViewer = NumberPanel.Template.FindName("NumberScrollViewer", NumberPanel) as ScrollViewer;

                return numberScrollViewer;
            }
        }

        private ScrollViewer operationScrollViewer;
        internal ScrollViewer _OperationScrollViewer
        {
            get
            {
                if (operationScrollViewer == null)
                    operationScrollViewer = OperationPanel.Template.FindName("OperationScrollViewer", OperationPanel) as ScrollViewer;

                return operationScrollViewer;
            }
        }

        private ScrollViewer valueScrollViewer;
        internal ScrollViewer _ValueScrollViewer
        {
            get
            {
                if (valueScrollViewer == null)
                    valueScrollViewer = ValuePanel.Template.FindName("ValueScrollViewer", ValuePanel) as ScrollViewer;

                return valueScrollViewer;
            }
        }

        public ObservableCollection<LineViewerLineData> LineDetails { get; set; }
            = new ObservableCollection<LineViewerLineData>();

        public int Count => LineDetails.Count;

        public void Clear()
        {
            LineDetails.Clear();
        }

        public void Add(DiffPiece diffPiece, string value, ChangeType changeType, UIElement source)
        {
            LineDetails.Add(new LineViewerLineData()
            {
                Number = diffPiece.Position,
                Piece = diffPiece,
                Segments = new List<LineViewerSegment>() {
                    new LineViewerSegment() {
                        TextChunk=value,
                        ChunkChange=changeType,
                        Source=source,
                        FallbackForeground = Foreground
                    } },
                ChangeType = changeType,
                Source = source,
                FallbackForeground = Foreground
            });
        }

        public void Add(DiffPiece diffPiece, List<LineViewerSegment> value, ChangeType changeType, UIElement source)
        {
            foreach (var item in value)
            {
                item.FallbackForeground = Foreground;
            }

            LineDetails.Add(new LineViewerLineData()
            {
                Number = diffPiece?.Position,
                Piece = diffPiece,
                Segments = value,
                ChangeType = changeType,
                Source = source,
                FallbackForeground = Foreground
            });
        }

        public void SetLineVisible(int index, bool visible)
        {
            try
            {
                var visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                LineDetails[index].Visible = visibility;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        public void ScrollToVerticalOffset(double offset)
        {
            _ValueScrollViewer.ScrollToVerticalOffset(offset);
        }

        internal void AdjustScrollView()
        {
            if (_ValueScrollViewer == null) return;

            var isV = _ValueScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
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

        private void NumberScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            
            var offset = _NumberScrollViewer.VerticalOffset;
            ScrollVertical(_ValueScrollViewer, offset);
            if (ScrollChanged != null)
                ScrollChanged(this, e);
        }

        private void OperationScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = _OperationScrollViewer.VerticalOffset;
            ScrollVertical(_ValueScrollViewer, offset);
            if (ScrollChanged != null)
                ScrollChanged(this, e);
        }

        private void ValueScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = _ValueScrollViewer.VerticalOffset;
            ScrollVertical(_NumberScrollViewer, offset);
            ScrollVertical(_OperationScrollViewer, offset);
            if (ScrollChanged != null)
                ScrollChanged(this, e);
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
