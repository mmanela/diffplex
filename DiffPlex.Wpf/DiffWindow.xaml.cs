using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using DiffPlex.DiffBuilder.Model;
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
        /// Gets or sets the old text.
        /// </summary>
        public string OldText
        {
            get => DiffView.OldText;
            set => DiffView.OldText = value;
        }

        /// <summary>
        /// Gets or sets the new text.
        /// </summary>
        public string NewText
        {
            get => DiffView.NewText;
            set => DiffView.NewText = value;
        }

        /// <summary>
        /// Gets or sets the header of the old text.
        /// </summary>
        public string OldTextHeader
        {
            get => DiffView.OldTextHeader;
            set => DiffView.OldTextHeader = value;
        }

        /// <summary>
        /// Gets or sets the header of the new text.
        /// </summary>
        public string NewTextHeader
        {
            get => DiffView.NewTextHeader;
            set => DiffView.NewTextHeader = value;
        }

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
        /// Gets or sets a value indicating whether need collapse unchanged sections.
        /// </summary>
        public bool IgnoreUnchanged
        {
            get => DiffView.IgnoreUnchanged;
            set => DiffView.IgnoreUnchanged = value;
        }

        /// <summary>
        /// Gets or sets the count of context line.
        /// The context line is the one unchanged arround others as their margin.
        /// </summary>
        public int LinesContext
        {
            get => DiffView.LinesContext;
            set => DiffView.LinesContext = value;
        }

        /// <summary>
        /// Gets the customized menu children.
        /// </summary>
        public UIElementCollection MenuChildren => MenuPanel.Children;

        /// <summary>
        /// Gets or sets the margin of the customized menu.
        /// </summary>
        public Thickness MenuMargin
        {
            get => MenuPanel.Margin;
            set => MenuPanel.Margin = value;
        }

        /// <summary>
        /// Gets the customized additional menu children.
        /// </summary>
        public UIElementCollection AddtionalMenuChildren => AdditionalMenuPanel.Children;

        /// <summary>
        /// Gets or sets the margin of the customized additional menu.
        /// </summary>
        public Thickness AddtionalMenuMargin
        {
            get => AdditionalMenuPanel.Margin;
            set => AdditionalMenuPanel.Margin = value;
        }


        /// <summary>
        /// Sets old text.
        /// </summary>
        /// <param name="value">The old text.</param>
        /// <param name="header">An optional header for old text.</param>
        public void SetOldText(string value, string header = null)
        {
            DiffView.OldText = value ?? string.Empty;
            leftFolderName = null;
            DiffView.OldTextHeader = leftFileName = header;
        }

        /// <summary>
        /// Sets new text.
        /// </summary>
        /// <param name="value">The new text.</param>
        /// <param name="header">An optional header for new text.</param>
        public void SetNewText(string value, string header = null)
        {
            DiffView.NewText = value ?? string.Empty;
            rightFolderName = null;
            DiffView.NewTextHeader = rightFileName = header;
        }

        /// <summary>
        /// Sets file contents as old and new text.
        /// </summary>
        /// <param name="oldFile">The old file information instance to read content.</param>
        /// <param name="newFile">The new file information instance to read content.</param>
        /// <returns>A token for the asynchronous operation.</returns>
        public async Task SetFiles(FileInfo oldFile, FileInfo newFile)
        {
            string left = string.Empty;
            string leftFile = null;
            string leftFolder = null;
            if (oldFile != null)
            {
                using var reader1 = oldFile.OpenText();
                left = await reader1.ReadToEndAsync();
                try
                {
                    leftFile = oldFile.Name;
                    leftFolder = oldFile.Directory?.Name;
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }

                if (oldFile == newFile)
                {
                    DiffView.OldText = DiffView.NewText = left;
                    leftFileName = rightFileName = leftFile;
                    leftFolderName = rightFolderName = leftFolder;
                    RefreshHeader();
                    return;
                }
            }

            string right = string.Empty;
            string rightFile = null;
            string rightFolder = null;
            if (newFile != null)
            {
                using var reader2 = newFile.OpenText();
                right = await reader2.ReadToEndAsync();
                try
                {
                    rightFile = newFile.Name;
                    rightFolder = newFile.Directory?.Name;
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
            }

            DiffView.OldText = left;
            DiffView.NewText = right;
            leftFileName = leftFile;
            leftFolderName = leftFolder;
            rightFileName = rightFile;
            rightFolderName = rightFolder;
            RefreshHeader();
        }

        /// <summary>
        /// Shows the context menu to open file.
        /// </summary>
        public void OpenOpenFileContextMenu()
        {
            OpenFileContextMenu.IsOpen = true;
        }

        /// <summary>
        /// Opens the context menu for view mode selection.
        /// </summary>
        public void OpenViewModeContextMenu()
        {
            DiffView.OpenViewModeContextMenu();
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

        /// <summary>
        /// Switches to the view of side-by-side (split) diff mode.
        /// </summary>
        public void ShowSideBySide()
        {
            DiffView.ShowSideBySide();
        }

        /// <summary>
        /// Switches to the view of inline (unified) diff mode.
        /// </summary>
        public void ShowInline()
        {
            DiffView.ShowInline();
        }
        /// <summary>
        /// Goes to a specific line.
        /// </summary>
        /// <param name="lineIndex">The index of the line to go to.</param>
        /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
        /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
        public bool GoTo(int lineIndex, bool isLeftLine = false)
        {
            return DiffView.GoTo(lineIndex, isLeftLine);
        }

        /// <summary>
        /// Goes to a specific line.
        /// </summary>
        /// <param name="line">The line to go to.</param>
        /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
        /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
        public bool GoTo(DiffPiece line, bool isLeftLine = false)
        {
            return DiffView.GoTo(line, isLeftLine);
        }

        /// <summary>
        /// Gets the line diff information.
        /// </summary>
        /// <param name="lineIndex">The index of the line to get information.</param>
        /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
        /// <returns>The line diff information instance; or null, if non-exists.</returns>
        public DiffPiece GetLine(int lineIndex, bool isLeftLine = false)
        {
            return DiffView.GetLine(lineIndex, isLeftLine);
        }

        /// <summary>
        /// Sets the style to the menu buttons.
        /// The buttons in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="style">The button style to set.</param>
        public void SetMenuButtonStyle(Style style)
        {
            OpenFileButton.Style = DiffButton.Style = FurtherActionsButton.Style = NextButton.Style = PreviousButton.Style = style;
        }

        /// <summary>
        /// Sets the control template to the menu buttons.
        /// The buttons in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="template">The control template to set.</param>
        public void SetMenuButtonTemplate(ControlTemplate template)
        {
            OpenFileButton.Template = DiffButton.Template = FurtherActionsButton.Template = NextButton.Template = PreviousButton.Template = template;
        }

        /// <summary>
        /// Sets the style to the menu text input boxes.
        /// The text input boxes in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="style">The button style to set.</param>
        public void SetMenuTextBoxStyle(Style style)
        {
            GoToText.Style = style;
        }

        /// <summary>
        /// Sets the control template to the menu text input boxes.
        /// The text input boxes in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="template">The control template to set.</param>
        public void SetMenuTextBoxTemlate(ControlTemplate template)
        {
            GoToText.Template = template;
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

        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiffView.IsInlineViewMode)
            {
                DiffView.ShowSideBySide();
                return;
            }

            DiffView.ShowInline();
        }

        private void FurtherActionsButton_Click(object sender, RoutedEventArgs e)
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

        private static TextFileInfo OpenTextFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All files|*.*|Plain text|*.txt;*.log;*.json;*.xml;*.csv;*.config;*.js;*.ts;*.jsx;*.tsx;*.py;*.cs;*.cpp;*.h;*.java;*.go;*.vb;*.vbs;*.xaml;*.md;*.svg;*.sql;*.csproj;*.cxproj;*.ini"
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
            catch (SecurityException)
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
            catch (SecurityException)
            {
            }

            return null;
        }
    }
}
