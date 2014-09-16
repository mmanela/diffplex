using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilverlightDiffer
{
    public partial class MainPage
    {
        private const int TimerPollFrequency = 200;
        private const int IdleTypingDelay = 500;

        private readonly TextBoxDiffRenderer diffRenderer;
        private readonly ScrollViewerSynchronizer scrollSynchronizer;
        private Timer diffTimer;
        private bool timerReady;
        private readonly List<Key> nonModifyingKeys;
        private DateTime lastKeyPress;


        public MainPage()
        {
            InitializeComponent();
            diffTimer = new Timer(DiffTimerCallback, null, 0, TimerPollFrequency);
            nonModifyingKeys = new List<Key>
                                   {
                                       Key.Shift,
                                       Key.Up,
                                       Key.Left,
                                       Key.Right,
                                       Key.Down,
                                       Key.PageDown,
                                       Key.PageUp,
                                       Key.Alt,
                                       Key.CapsLock,
                                       Key.Ctrl,
                                       Key.Escape
                                   };

            scrollSynchronizer = new ScrollViewerSynchronizer(new List<ScrollViewer> {LeftScroller, RightScroller});
            diffRenderer = new TextBoxDiffRenderer(LeftDiffGrid, LeftBox, RightDiffGrid, RightBox);
        }

        private void DiffTimerCallback(object state)
        {
            if (timerReady && TimeSpan.FromTicks(DateTime.Now.Ticks - lastKeyPress.Ticks).TotalMilliseconds > IdleTypingDelay)
            {
                Dispatcher.BeginInvoke(diffRenderer.GenerateDiffView);
                timerReady = false;
            }
        }

        private void GenerateDiffButton_Click(object sender, RoutedEventArgs e)
        {
            diffRenderer.GenerateDiffView();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (nonModifyingKeys.Contains(e.Key))
                return;

            lastKeyPress = DateTime.Now;
            timerReady = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }


        private static double? GetOverride(TextBox box)
        {
            double? overrideValue = null;
            if (!string.IsNullOrEmpty(box.Text))
            {
                try
                {
                    overrideValue = double.Parse(box.Text);
                }
                catch (Exception)
                {
                }
            }
            return overrideValue;
        }

        private void Override_TextChanged(object sender, TextChangedEventArgs e)
        {
            var overrideBox = sender as TextBox;
            if (overrideBox == null) return;

            switch (overrideBox.Name)
            {
                case "linePaddingOverride":
                    diffRenderer.LinePaddingOverride = GetOverride(overrideBox);
                    break;
                case "topOffsetOverride":
                    diffRenderer.TopOffsetOverride = GetOverride(overrideBox);
                    break;
                case "charWidthOverride":
                    diffRenderer.CharacterWidthOverride = GetOverride(overrideBox);
                    break;
                case "leftOffsetOverride":
                    diffRenderer.LeftOffsetOverride = GetOverride(overrideBox);
                    break;
                default:
                    break;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (sender == null) return;
            diffRenderer.ShowVisualAids = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value;
        }
    }
}