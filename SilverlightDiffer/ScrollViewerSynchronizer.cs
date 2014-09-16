using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SilverlightDiffer
{
    public class ScrollViewerSynchronizer
    {
        private readonly List<ScrollViewer> scrollViewers;
        private readonly Dictionary<ScrollBar, ScrollViewer> verticalScrollerViewers = new Dictionary<ScrollBar, ScrollViewer>();
        private readonly Dictionary<ScrollBar, ScrollViewer> horizontalScrollerViewers = new Dictionary<ScrollBar, ScrollViewer>();
        private double verticalScrollOffset;
        private double horizontalScrollOffset;

        public ScrollViewerSynchronizer(List<ScrollViewer> scrollViewers)
        {
            this.scrollViewers = scrollViewers;
            scrollViewers.ForEach(x => x.Loaded += Scroller_Loaded);
        }

        private void Scroller_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = (ScrollViewer) sender;
            scrollViewer.ScrollToVerticalOffset(verticalScrollOffset);
            scrollViewer.ScrollToHorizontalOffset(horizontalScrollOffset);
            scrollViewer.Opacity = 1;
            if (verticalScrollerViewers.Count > 0)
                scrollViewer.ScrollToVerticalOffset(verticalScrollOffset);
            scrollViewer.ApplyTemplate();


            var scrollViewerRoot = (FrameworkElement) VisualTreeHelper.GetChild(scrollViewer, 0);
            var horizontalScrollBar = (ScrollBar) scrollViewerRoot.FindName("HorizontalScrollBar");
            var verticalScrollBar = (ScrollBar) scrollViewerRoot.FindName("VerticalScrollBar");

            if (!horizontalScrollerViewers.Keys.Contains(horizontalScrollBar))
            {
                horizontalScrollerViewers.Add(horizontalScrollBar, scrollViewer);
            }

            if (!verticalScrollerViewers.Keys.Contains(verticalScrollBar))
            {
                verticalScrollerViewers.Add(verticalScrollBar, scrollViewer);
            }

            if (horizontalScrollBar != null)
            {
                horizontalScrollBar.Scroll += HorizontalScrollBar_Scroll;
                horizontalScrollBar.ValueChanged += HorizontalScrollBar_ValueChanged;
            }

            if (verticalScrollBar != null)
            {
                verticalScrollBar.Scroll += VerticalScrollBar_Scroll;
                verticalScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;
            }
        }

        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var changedScrollBar = sender as ScrollBar;
            var changedScrollViewer = verticalScrollerViewers[changedScrollBar];
            Scroll(changedScrollViewer);
        }

        private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            var changedScrollBar = sender as ScrollBar;
            var changedScrollViewer = verticalScrollerViewers[changedScrollBar];
            Scroll(changedScrollViewer);
        }

        private void HorizontalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var changedScrollBar = sender as ScrollBar;
            var changedScrollViewer = horizontalScrollerViewers[changedScrollBar];
            Scroll(changedScrollViewer);
        }

        private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            var changedScrollBar = sender as ScrollBar;
            var changedScrollViewer = horizontalScrollerViewers[changedScrollBar];
            Scroll(changedScrollViewer);
        }

        private void Scroll(ScrollViewer changedScrollViewer)
        {
            verticalScrollOffset = changedScrollViewer.VerticalOffset;
            horizontalScrollOffset = changedScrollViewer.HorizontalOffset;

            foreach (var scrollViewer in scrollViewers.Where(s => s != changedScrollViewer))
            {
                if (scrollViewer.VerticalOffset != changedScrollViewer.VerticalOffset)
                {
                    scrollViewer.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
                }

                if (scrollViewer.HorizontalOffset != changedScrollViewer.HorizontalOffset)
                {
                    scrollViewer.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
                }
            }
        }
    }
}