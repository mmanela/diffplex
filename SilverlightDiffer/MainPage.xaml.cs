using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DiffPlex;
using DiffPlex.TextDiffer;
using DiffPlex.TextDiffer.Model;

namespace SilverlightDiffer
{
    public partial class MainPage
    {
        private const char ImaginaryLineCharacter = '\u200B';
        TextDiffBuilder differ = new TextDiffBuilder(new Differ());
        object mutex = new object();
        private int inDiff;

        public MainPage()
        {
            InitializeComponent();
        }


        private void GenerateDiffButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateDiffView();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           // GenerateDiffView();
        }

        void GenerateDiffView()
        {
            if (inDiff > 0) return;
            lock (mutex)
            {
                if (inDiff > 0) return;
                inDiff++;
            }

            var leftContent = LeftBox.Text.Replace(ImaginaryLineCharacter + "\r","");
            var rightContent = RightBox.Text.Replace(ImaginaryLineCharacter + "\r", "");

            ThreadPool.QueueUserWorkItem(state =>
                                             {
                                                 var diffRes = differ.BuildDiffModel(leftContent, rightContent);
                                                 Dispatcher.BeginInvoke(() => GenerateDiffLines(diffRes.OldText, diffRes.NewText));
                                                 inDiff = 0;
                                             });





        }

        private void GenerateDiffLines(DiffPaneModel leftDiff, DiffPaneModel rightDiff)
        {
            RenderDiffLinesInGrid(LeftDiffGrid, LeftBox, leftDiff);
            RenderDiffLinesInGrid(RightDiffGrid, RightBox, rightDiff);
        }

        private void ClearDiffLinesFromGrid(Grid grid)
        {
            var rectangles = grid.Children.Where(x => x.GetType() == typeof(Rectangle)).ToList();
            foreach (var rect in rectangles)
            {
                grid.Children.Remove(rect);
            }
        }

        private void RenderDiffLinesInGrid(Grid grid, TextBox textBox, DiffPaneModel diffModel)
        {
            ClearDiffLinesFromGrid(grid);

            double? paddingOverride = null; 
            if(!string.IsNullOrEmpty(linePaddingOverride.Text))
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

                    AddImaginaryLine(textBox,lineNumber);
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
            var selectedText = textBox.SelectedText;
            var lines = textBox.Text.Split('\r').ToList();
            lines.Insert(lineNumber, ImaginaryLineCharacter.ToString());
            textBox.Text = lines.Aggregate((x, y) => x + '\r' + y);

        }
    }



    public class TextSpan
    {
        public TextSpan(int lineStart, int charStart, int lineEnd, int charEnd)
        {
            LineStart = lineStart;
            LineEnd = lineEnd;
            CharacterStart = charStart;
            CharacterEnd = charEnd;
        }
        public int LineStart { get; set; }
        public int LineEnd { get; set; }
        public int CharacterStart { get; set; }
        public int CharacterEnd { get; set; }
    }

}