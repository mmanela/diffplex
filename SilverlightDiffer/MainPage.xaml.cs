using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DiffPlex;
using DiffPlex.TextDiffer;
using DiffPlex.TextDiffer.Model;

namespace SilverlightDiffer
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }


        private void GenerateDiffButton_Click(object sender, RoutedEventArgs e)
        {
            var leftContent = LeftBox.Text;
            var rightContent = RightBox.Text;

            var differ = new TextDiffBuilder(new Differ());
            var diffRes = differ.BuildDiffModel(leftContent, rightContent);
            //LeftBlock.Visibility = Visibility.Visible;
            //RightBlock.Visibility = Visibility.Visible;

            //GenerateDiffBlock(LeftBlock, diffRes.OldText);
            //GenerateDiffBlock(RightBlock, diffRes.NewText);

            LeftBox.Visibility = Visibility.Collapsed;
            RightBox.Visibility = Visibility.Collapsed;
        }

        private void GenerateDiffBlock(TextBlock block, DiffPaneModel diffModel)
        {
            block.Inlines.Clear();
            foreach (var line in diffModel.Lines)
            {
                var run = new Run();
                run.Text = line.Text + Environment.NewLine;
                if (line.Type == ChangeType.Deleted)
                    run.Foreground = new SolidColorBrush(Colors.Red);
                else if (line.Type == ChangeType.Inserted)
                    run.Foreground = new SolidColorBrush(Colors.Yellow);

                block.Inlines.Add(run);
            }
        }

    }
}