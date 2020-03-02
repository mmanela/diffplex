using System;
using System.Collections.Generic;
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

using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Wpf.Controls;

namespace DiffPlex.Wpf.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SideBySideDiffModel sideBySide;
        private DiffPaneModel inline;

        public MainWindow()
        {
            InitializeComponent();

            SideBySideDiff.SetDiffModel(new Differ(), TestData.OldText, TestData.NewText);
        }

        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (SideBySideDiff.Visibility == Visibility.Visible)
            {
                SideBySideDiff.Visibility = Visibility.Collapsed;
                InlineDiff.Visibility = Visibility.Visible;
                if (inline == null)
                {
                    var builder = new InlineDiffBuilder(new Differ());
                    inline = builder.BuildDiffModel(TestData.DuplicateText(TestData.OldText, 50), TestData.DuplicateText(TestData.NewText, 50));
                }

                InlineDiff.DiffModel = inline;
                return;
            }

            InlineDiff.Visibility = Visibility.Collapsed;
            SideBySideDiff.Visibility = Visibility.Visible;
            if (sideBySide == null)
            {
                var builder = new SideBySideDiffBuilder(new Differ());
                sideBySide = builder.BuildDiffModel(TestData.DuplicateText(TestData.OldText, 50), TestData.DuplicateText(TestData.NewText, 50));
            }

            SideBySideDiff.DiffModel = sideBySide;
            //SideBySideDiff.SetDiffModel(new Differ(), TestData.DuplicateText(TestData.OldText, 20), TestData.DuplicateText(TestData.NewText, 20));
        }
    }
}
