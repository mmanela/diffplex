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
        private const char ImaginaryLineCharacter = '\u202B';


        private readonly TextDiffBuilder differ = new TextDiffBuilder(new Differ());
        private readonly object mutex = new object();
        private bool inDiff;
        private Timer diffTimer;
        private bool timerReady;
        private readonly List<Key> nonModifyingKeys;
        private DateTime lastKeyPress;

        private List<FontInfo> fontInfos;
        private FontInfo currentFont;

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

            fontInfos = new List<FontInfo>
                              {
                                  new FontInfo("Courier New",1.466,6.62,3.5),
                                  new FontInfo("Consolas",1.88 )
                              };
            currentFont = fontInfos.Single(x => x.FontFamily.Equals(LeftBox.FontFamily.Source, StringComparison.OrdinalIgnoreCase));
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
            GenerateDiffPanes(diffRes.OldText, diffRes.NewText);
            inDiff = false;
        }

        private void GenerateDiffPanes(DiffPaneModel leftDiff, DiffPaneModel rightDiff)
        {
            RenderDiffLines(LeftDiffGrid, LeftBox, leftDiff);
            RenderDiffLines(RightDiffGrid, RightBox, rightDiff);
        }

        private void ClearDiffLines(Grid grid)
        {
            var rectangles = grid.Children.Where(x => x.GetType() == typeof(Rectangle)).ToList();
            foreach (var rect in rectangles)
            {
                grid.Children.Remove(rect);
            }
        }

        private double? GetOverride(TextBox box)
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

        private void RenderDiffLines(Grid grid, TextBox textBox, DiffPaneModel diffModel)
        {
            ClearDiffLines(grid);


            var lineNumber = 0;
            foreach (var line in diffModel.Lines)
            {
                var fillColor = new SolidColorBrush(Colors.Transparent);
                if (line.Type == ChangeType.Deleted)
                    fillColor = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100));
                else if (line.Type == ChangeType.Inserted)
                    fillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                else if (line.Type == ChangeType.Unchanged)
                    fillColor = new SolidColorBrush(Colors.White);
                else if (line.Type == ChangeType.Modified)
                {
                    if(currentFont.IsMonoSpaced)
                        RenderDiffWords(grid, textBox, line, lineNumber);
                    fillColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 255));
                }
                else if (line.Type == ChangeType.Imaginary)
                {
                    fillColor = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));

                    AddImaginaryLine(textBox, lineNumber);
                }

                if (ShowVisualAids.IsChecked == true)
                {
                    if (lineNumber % 2 == 0)
                        fillColor = new SolidColorBrush(Colors.Cyan);
                    else
                    {
                        fillColor = new SolidColorBrush(Colors.Gray);
                    }
                }

                PlaceRectangleInGrid(textBox, grid, lineNumber, fillColor, 0, null);
                lineNumber++;
            }
        }

        private void RenderDiffWords(Grid grid, TextBox textBox, DiffPiece line, int lineNumber)
        {
            var characterWidthOverride = GetOverride(charWidthOverride);
            var characterLeftOffsetOveride = GetOverride(leftOffsetOverride);
            var charPos = 0;
            var characterWidth = characterWidthOverride ?? currentFont.CharacterWidth;
            var leftOffset = characterLeftOffsetOveride ?? currentFont.LeftOffset;
            foreach (var word in line.SubPieces)
            {

                SolidColorBrush fillColor;
                if (word.Type == ChangeType.Deleted)
                    fillColor = new SolidColorBrush(Color.FromArgb(255, 200, 100, 100));
                else if (word.Type == ChangeType.Inserted)
                    fillColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 150));
                else if (word.Type == ChangeType.Imaginary)
                    continue;
                else
                    fillColor = new SolidColorBrush(Colors.Transparent);

                var left = characterWidth * charPos + leftOffset;
                var wordWidth = characterWidth * word.Text.Length;
                PlaceRectangleInGrid(textBox, grid, lineNumber, fillColor, left, wordWidth);

                charPos += word.Text.Length;
            }
        }

        private void PlaceRectangleInGrid(TextBox textBox, Grid grid, int lineNumber, SolidColorBrush fillColor, double left, double? width)
        {
            
            var paddingOverride = GetOverride(linePaddingOverride);
            var offsetOverride = GetOverride(topOffsetOverride);


            var rectLineHeight = textBox.FontSize + (paddingOverride ?? currentFont.LinePadding);
            double rectTopOffset = offsetOverride ?? 3;

            var offset = rectLineHeight * lineNumber + rectTopOffset;
            var floor = Math.Floor(offset);
            var fraction = offset - floor;

            var rectangle = new Rectangle
            {
                Fill = fillColor,
                Width = width ?? Double.NaN,
                Height = rectLineHeight + fraction,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = width.HasValue ? HorizontalAlignment.Left : HorizontalAlignment.Stretch,
                Margin = new Thickness(left, floor, 0, 0)
            };

            grid.Children.Insert(0, rectangle);
        }

        public void AddImaginaryLine(TextBox textBox, int lineNumber)
        {
            var selectionStart = textBox.SelectionStart;
            var lines = new List<string>();
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                lines = textBox.Text.Split('\r').ToList();
                var insertPosition = 0;
                for (var i = 0; i < lineNumber; i++)
                {
                    insertPosition += lines[i].Length + 1;
                }
                if (selectionStart >= insertPosition)
                    selectionStart += 2;
            }
            lines.Insert(lineNumber, ImaginaryLineCharacter.ToString());
            textBox.Text = lines.Aggregate((x, y) => x + '\r' + y);
            textBox.SelectionStart = selectionStart;
        }

        public void StripImaginaryLinesAndCharacters(TextBox textBox)
        {
            var selectionStart = textBox.SelectionStart;
            var offset = 0;
            for (var i = 0; i < textBox.Text.Length - 1 && i < selectionStart; i++)
            {
                if (i == 0 || textBox.Text[i] == '\r')
                {
                    var nextNewLine = textBox.Text.IndexOf('\r', i + 1);
                    var nextImaginary = textBox.Text.IndexOf(ImaginaryLineCharacter, i);
                    if (nextImaginary != -1 && (nextNewLine == -1 || nextNewLine > nextImaginary) && nextImaginary < selectionStart)
                        offset++;
                }
            }
            selectionStart -= offset;


            var lines = textBox.Text.Split('\r').Where(x => !x.Equals(ImaginaryLineCharacter.ToString()));
            var text = lines.Count() == 0 ? "" : lines.Aggregate((x, y) => x + '\r' + y);

            textBox.Text = text.Replace(ImaginaryLineCharacter.ToString(), "");
            textBox.SelectionStart = selectionStart;
        }
    }
}