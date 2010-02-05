using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DiffPlex;
using DiffPlex.TextDiffer;
using DiffPlex.TextDiffer.Model;

namespace SilverlightDiffer
{
    public partial class MainPage
    {
        private const int TimerPollFrequency = 200;
        private const int IdleTypingDelay = 500;
        private const char ImaginaryLineCharacter = '\u200B';
        
        
        private readonly TextDiffBuilder differ = new TextDiffBuilder(new Differ());
        private readonly object mutex = new object();
        private bool inDiff;
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
        }

        private void DiffTimerCallback(object state)
        {
            if (timerReady && TimeSpan.FromTicks(DateTime.Now.Ticks - lastKeyPress.Ticks).TotalMilliseconds > IdleTypingDelay)
            {
                Dispatcher.BeginInvoke(GenerateDiffView);
                timerReady = false;
            }
        }

        private void GenerateDiffButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateDiffView();
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

        private void GenerateDiffView()
        {
            if (inDiff) return;
            lock (mutex)
            {
                if (inDiff) return;
                inDiff = true;
            }

            StripImaginaryLinesAndCharacters(LeftBox);
            StripImaginaryLinesAndCharacters(RightBox);
            var leftContent = LeftBox.Text;
            var rightContent = RightBox.Text;


            var diffRes = differ.BuildDiffModel(leftContent, rightContent);
            GenerateDiffLines(diffRes.OldText, diffRes.NewText);
            inDiff = false;
        }

        private void GenerateDiffLines(DiffPaneModel leftDiff, DiffPaneModel rightDiff)
        {
            RenderDiffLinesInGrid(LeftDiffGrid, LeftBox, leftDiff);
            RenderDiffLinesInGrid(RightDiffGrid, RightBox, rightDiff);
        }

        private void ClearDiffLinesFromGrid(Grid grid)
        {
            var rectangles = grid.Children.Where(x => x.GetType() == typeof (Rectangle)).ToList();
            foreach (var rect in rectangles)
            {
                grid.Children.Remove(rect);
            }
        }

        private void RenderDiffLinesInGrid(Grid grid, TextBox textBox, DiffPaneModel diffModel)
        {
            ClearDiffLinesFromGrid(grid);

            double? paddingOverride = null;
            if (!string.IsNullOrEmpty(linePaddingOverride.Text))
            {
                try
                {
                    paddingOverride = double.Parse(linePaddingOverride.Text);
                }
                catch (Exception)
                {
                }
            }

            var linePadding = 1.88; // this is specific to Consolas
            var rectLineHeight = textBox.FontSize + (paddingOverride ?? linePadding);
            const int rectTopOffset = 3;
            var lineNumber = 0;
            foreach (var line in diffModel.Lines)
            {
                //textBox.FontSize
                var fillColor = new SolidColorBrush(Colors.Transparent);
                if (line.Type == ChangeType.Deleted)
                    fillColor = new SolidColorBrush(Colors.Red);
                else if (line.Type == ChangeType.Inserted)
                    fillColor = new SolidColorBrush(Colors.Yellow);
                else if (line.Type == ChangeType.Unchanged)
                    fillColor = new SolidColorBrush(Colors.White);
                else if (line.Type == ChangeType.Modified)
                    fillColor = new SolidColorBrush(Colors.Cyan);
                else if (line.Type == ChangeType.Imaginary)
                {
                    fillColor = new SolidColorBrush(Colors.LightGray);

                    AddImaginaryLine(textBox, lineNumber);
                }

                if (paddingOverride != null)
                {
                    if (lineNumber % 2 == 0)
                        fillColor = new SolidColorBrush(Colors.Cyan);
                    else
                    {
                        fillColor = new SolidColorBrush(Colors.Gray);
                    }
                }

                var rectangle = new Rectangle
                                    {
                                        Fill = fillColor,
                                        Height = rectLineHeight,
                                        VerticalAlignment = VerticalAlignment.Top,
                                        HorizontalAlignment = HorizontalAlignment.Stretch,
                                        Margin = new Thickness(0, rectLineHeight * lineNumber + rectTopOffset, 0, 0)
                                    };
                grid.Children.Insert(0, rectangle);
                lineNumber++;
            }
        }

        public void AddImaginaryLine(TextBox textBox, int lineNumber)
        {
            var selectionStart = textBox.SelectionStart;
            var selectionLength = textBox.SelectionLength;

            var lines = textBox.Text.Split('\r').ToList();
            var insertPosition = 0;
            for (var i = 0; i < lineNumber; i++)
            {
                insertPosition += lines[i].Length + 1;
            }
            if (selectionStart >= insertPosition)
                selectionStart += 2;
            lines.Insert(lineNumber, ImaginaryLineCharacter.ToString());
            textBox.Text = lines.Aggregate((x, y) => x + '\r' + y);
            textBox.SelectionStart = selectionStart;
        }

        public void StripImaginaryLinesAndCharacters(TextBox textBox)
        {
            var selectionStart = textBox.SelectionStart;
            var offset = 0;
            for (var i = 0; i < textBox.Text.Length && i < selectionStart; i++)
                if (textBox.Text[i] == '\r' || textBox.Text[i] == ImaginaryLineCharacter)
                    offset++;
            selectionStart -= offset;


            var lines = textBox.Text.Split('\r').Where(x => !x.Equals(ImaginaryLineCharacter.ToString()));
            var text = lines.Count() == 0 ? "" : lines.Aggregate((x, y) => x + '\r' + y);

            textBox.Text = text.Replace(ImaginaryLineCharacter.ToString(), "");
            textBox.SelectionStart = selectionStart;
        }
    }
}