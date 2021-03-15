using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

using DiffPlex.Wpf.Controls;

namespace DiffPlex.Wpf
{
    /// <summary>
    /// Diff window.
    /// </summary>
    public partial class DiffWindow : Window
    {
        /// <summary>
        /// The information for the text file.
        /// </summary>
        internal class TextFileInfo
        {
            /// <summary>
            /// Gets or sets the file name.
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Gets or sets the folder name.
            /// </summary>
            public string FolderName { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public string Value { get; set; }
        }

        private string leftFileName;
        private string leftFolderName;
        private string rightFileName;
        private string rightFolderName;

        /// <summary>
        /// Initializes a new instance of the DiffWindow class.
        /// </summary>
        public DiffWindow()
        {
            InitializeComponent();

            Name = Resource.Diff;
            var now = DateTime.Now;
            var isDark = now.Hour < 6 || now.Hour >= 18;
            Foreground = new SolidColorBrush(isDark ? Color.FromRgb(240, 240, 240) : Color.FromRgb(32, 32, 32));
            Background = new SolidColorBrush(isDark ? Color.FromRgb(32, 32, 32) : Color.FromRgb(251, 251, 251));
            DiffView.SetHeaderAsLeftToRight();
            OpenFileButton.Content = Resource.OpenFile;
            OpenLeftFileMenuItem.Header = Resource.Left;
            OpenRightFileMenuItem.Header = Resource.Right;
            DiffButton.Content = Resource.SwitchViewMode;
            GoToLabel.Content = Resource.GoTo;
            PreviousButton.ToolTip = Resource.Previous;
            NextButton.ToolTip = Resource.Next;
        }

        /// <summary>
        /// Gets the diff viewer.
        /// </summary>
        public DiffViewer Core => DiffView;

        /// <summary>
        /// Gets or sets a value indicating whether need set headers automatically.
        /// </summary>
        public bool AutoSetHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether show open file button.
        /// </summary>
        public bool IsOpenFileButtonVisbile
        {
            get => OpenFileButton.Visibility == Visibility.Visible;
            set => OpenFileButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Gets a value indicating whether it is side-by-side (split) view mode.
        /// </summary>
        public bool IsSideBySideViewMode => Core.IsSideBySideViewMode;

        /// <summary>
        /// Gets the customized menu children.
        /// </summary>
        public UIElementCollection MenuChildren => MenuPanel.Children;

        /// <summary>
        /// Gets the customized additional menu children.
        /// </summary>
        public UIElementCollection AddtionalMenuChildren => AdditionalMenuPanel.Children;

        /// <summary>
        /// Sets old text.
        /// </summary>
        /// <param name="value">The old text.</param>
        /// <param name="header">An optional header for old text.</param>
        public void SetOldText(string value, string header = null)
        {
            DiffView.OldText = value ?? string.Empty;
            leftFileName = header;
            leftFolderName = null;
            DiffView.OldTextHeader = header;
        }

        /// <summary>
        /// Sets new text.
        /// </summary>
        /// <param name="value">The new text.</param>
        /// <param name="header">An optional header for new text.</param>
        public void SetNewText(string value, string header = null)
        {
            DiffView.NewText = value ?? string.Empty;
            rightFileName = header;
            rightFolderName = null;
            DiffView.NewTextHeader = header;
        }

        /// <summary>
        /// Shows the context menu to open file.
        /// </summary>
        public void OpenOpenFileContextMenu()
        {
            OpenFileContextMenu.IsOpen = true;
        }

        /// <summary>
        /// Pops up a file dialog to open file on both of left and right.
        /// </summary>
        public void OpenFileOnBoth()
        {
            var text = OpenLeftFileInternal();
            DiffView.NewText = text;
            rightFileName = leftFileName;
            rightFolderName = leftFolderName;
            RefreshHeader();
        }

        /// <summary>
        /// Pops up a file dialog to open file on left.
        /// </summary>
        public void OpenFileOnLeft()
        {
            OpenLeftFileInternal();
            if (DiffView.NewText != null) return;
            DiffView.NewText = string.Empty;
            rightFileName = Resource.Empty;
            rightFolderName = null;
            RefreshHeader();
        }

        /// <summary>
        /// Pops up a file dialog to open file on right.
        /// </summary>
        public void OpenFileOnRight()
        {
            var text = OpenTextFile();
            if (text?.Value == null) return;
            DiffView.NewText = text.Value;
            rightFileName = text.FileName?.Trim();
            rightFolderName = text.FolderName?.Trim();
            if (DiffView.OldText != null)
            {
                RefreshHeader();
                return;
            }

            DiffView.OldText = string.Empty;
            leftFileName = Resource.Empty;
            leftFolderName = null;
            RefreshHeader();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiffView.OldText == null)
            {
                var text = OpenLeftFileInternal();
                if (text == null || DiffView.NewText != null) return;
                DiffView.NewText = text;
                rightFileName = leftFileName;
                rightFolderName = leftFolderName;
                RefreshHeader();
            }
            else if (DiffView.NewText == null)
            {
                OpenRightFileMenuItem_Click(sender, e);
            }
            else
            {
                OpenFileContextMenu.IsOpen = true;
            }
        }

        private string OpenLeftFileInternal()
        {
            var text = OpenTextFile();
            if (text?.Value == null) return null;
            DiffView.OldText = text.Value;
            leftFileName = text.FileName?.Trim();
            leftFolderName = text.FolderName?.Trim();
            if (DiffView.NewText != null) RefreshHeader();
            return text.Value;
        }

        private void OpenLeftFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenLeftFileInternal();
            if (DiffView.NewText != null) return;
            DiffView.NewText = string.Empty;
            rightFileName = Resource.Empty;
            rightFolderName = null;
            RefreshHeader();
        }

        private void OpenRightFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var text = OpenTextFile();
            if (text?.Value == null) return;
            DiffView.NewText = text.Value;
            rightFileName = text.FileName?.Trim();
            rightFolderName = text.FolderName?.Trim();
            if (DiffView.OldText != null)
            {
                RefreshHeader();
                return;
            }

            DiffView.OldText = string.Empty;
            leftFileName = Resource.Empty;
            leftFolderName = null;
            RefreshHeader();
        }

        private void RefreshHeader()
        {
            if (!AutoSetHeader) return;
            DiffView.SetHeaderAsLeftToRight();
            if (leftFileName != rightFileName)
            {
                if (!string.IsNullOrEmpty(leftFileName)) DiffView.OldTextHeader = leftFileName;
                if (!string.IsNullOrEmpty(rightFileName)) DiffView.NewTextHeader = rightFileName;
                return;
            }

            if (leftFolderName == rightFolderName) return;
            if (!string.IsNullOrEmpty(leftFolderName)) DiffView.OldTextHeader = leftFolderName;
            if (!string.IsNullOrEmpty(rightFolderName)) DiffView.NewTextHeader = rightFolderName;
            return;
        }

        private TextFileInfo OpenTextFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All|*.*|Plain text|*.txt;*.log;*.json;*.xml;*.csv;*.config;*.js;*.ts;*.jsx;*.tsx;*.py;*.cs;*.cpp;*.h;*.java;*.go;*.vb;*.vbs;*.xaml;*.md;*.svg;*.sql;*.csproj;*.cxproj;*.ini"
            };
            if (dialog.ShowDialog() != true) return null;
            var fileName = dialog.FileName;
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            string name = null;
            string folder = null;
            try
            {
                var file = new FileInfo(fileName);
                name = file.Name;
                folder = file.Directory?.Name;
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (System.Security.SecurityException)
            {
            }

            try
            {
                return new TextFileInfo
                {
                    FileName = name,
                    FolderName = folder,
                    Value = File.ReadAllText(fileName)
                };
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (System.Security.SecurityException)
            {
            }

            return null;
        }

        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiffView.IsInlineViewMode)
            {
                DiffView.ShowSideBySide();
                return;
            }

            DiffView.ShowInline();
        }

        private void FutherActionsButton_Click(object sender, RoutedEventArgs e)
        {
            DiffView.OpenViewModeContextMenu();
        }

        private void GoToText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = GoToText.Text?.Trim();
            if (string.IsNullOrEmpty(s)) return;
            if (!int.TryParse(s, out var i)) return;
            DiffView.GoTo(i, string.IsNullOrEmpty(DiffView.NewText));
        }

        private void GoToText_LostFocus(object sender, RoutedEventArgs e)
        {
            GoToText.Text = null;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var isLeft = string.IsNullOrEmpty(DiffView.NewText);
            var pageSize = DiffView.GetLinesInViewport(isLeft, VisibilityLevels.All).Count();
            var lines = DiffView.GetLinesBeforeViewport(isLeft, VisibilityLevels.All).Reverse().ToList();
            if (lines.Count < pageSize)
            {
                DiffView.GoTo(lines.LastOrDefault());
                return;
            }

            var line = lines.Take(pageSize).Reverse().FirstOrDefault(ele => ele.Type != DiffBuilder.Model.ChangeType.Unchanged);
            if (line == null) line = lines.FirstOrDefault(ele => ele.Type != DiffBuilder.Model.ChangeType.Unchanged);
            DiffView.GoTo(line, isLeft);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var isLeft = string.IsNullOrEmpty(DiffView.NewText);
            var line = DiffView.GetLinesAfterViewport(isLeft, VisibilityLevels.All).FirstOrDefault(ele => ele.Type != DiffBuilder.Model.ChangeType.Unchanged);
            DiffView.GoTo(line, isLeft);
        }
    }
}
