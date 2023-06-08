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
            get => DiffView.IsOpenFileButtonVisible;
            set => DiffView.IsOpenFileButtonVisible = value;
        }

        /// <summary>
        /// Gets a value indicating whether it is side-by-side (split) view mode.
        /// </summary>
        public bool IsSideBySideViewMode => DiffView.IsSideBySideViewMode;

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
        public UIElementCollection MenuChildren => DiffView.MenuChildren;

        /// <summary>
        /// Gets or sets the margin of the customized menu.
        /// </summary>
        public Thickness MenuMargin
        {
            get => DiffView.MenuMargin;
            set => DiffView.MenuMargin = value;
        }

        /// <summary>
        /// Gets the customized additional menu children.
        /// </summary>
        public UIElementCollection AdditionalMenuChildren => DiffView.AdditionalMenuChildren;

        /// <summary>
        /// Gets or sets the margin of the customized additional menu.
        /// </summary>
        public Thickness AddtionalMenuMargin
        {
            get => DiffView.AdditionalMenuMargin;
            set => DiffView.AdditionalMenuMargin = value;
        }

        /// <summary>
        /// Sets the text.
        /// </summary>
        /// <param name="left">The old text.</param>
        /// <param name="right">The new text.</param>
        public void SetText(string left, string right)
            => DiffView.SetText(left, right);

        /// <summary>
        /// Sets old text.
        /// </summary>
        /// <param name="value">The old text.</param>
        /// <param name="header">An optional header for old text.</param>
        public void SetOldText(string value, string header = null)
            => DiffView.SetOldText(value, header);

        /// <summary>
        /// Sets new text.
        /// </summary>
        /// <param name="value">The new text.</param>
        /// <param name="header">An optional header for new text.</param>
        public void SetNewText(string value, string header = null)
            => DiffView.SetNewText(value, header);

        /// <summary>
        /// Sets file contents as old and new text.
        /// </summary>
        /// <param name="oldFile">The old file information instance to read content.</param>
        /// <param name="newFile">The new file information instance to read content.</param>
        /// <returns>A token for the asynchronous operation.</returns>
        public Task SetFiles(FileInfo oldFile, FileInfo newFile)
            => DiffView.SetFiles(oldFile, newFile);

        /// <summary>
        /// Shows the context menu to open file.
        /// </summary>
        public void ShowOpenFileContextMenu()
            => DiffView.ShowOpenFileContextMenu();

        /// <summary>
        /// Opens the context menu for view mode selection.
        /// </summary>
        public void OpenViewModeContextMenu()
            => DiffView.OpenViewModeContextMenu();

        /// <summary>
        /// Pops up a file dialog to open file on both of left and right.
        /// </summary>
        public void OpenFileOnBoth()
            => DiffView.OpenFileOnBoth();

        /// <summary>
        /// Pops up a file dialog to open file on left.
        /// </summary>
        public void OpenFileOnLeft()
            => DiffView.OpenFileOnLeft();

        /// <summary>
        /// Pops up a file dialog to open file on left.
        /// </summary>
        /// <param name="header">The optional header.</param>
        /// <param name="file">The file opened.</param>
        public void OpenFileOnLeft(string header, out FileInfo file)
            => DiffView.OpenFileOnLeft(header, out file);

        /// <summary>
        /// Pops up a file dialog to open file on right.
        /// </summary>
        public void OpenFileOnRight()
            => DiffView.OpenFileOnRight();

        /// <summary>
        /// Pops up a file dialog to open file on right.
        /// </summary>
        /// <param name="header">The optional header.</param>
        /// <param name="file">The file opened.</param>
        public void OpenFileOnRight(string header, out FileInfo file)
            => DiffView.OpenFileOnRight(header, out file);

        /// <summary>
        /// Switches to the view of side-by-side (split) diff mode.
        /// </summary>
        public void ShowSideBySide()
            => DiffView.ShowSideBySide();

        /// <summary>
        /// Switches to the view of inline (unified) diff mode.
        /// </summary>
        public void ShowInline()
            => DiffView.ShowInline();

        /// <summary>
        /// Goes to a specific line.
        /// </summary>
        /// <param name="lineIndex">The index of the line to go to.</param>
        /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
        /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
        public bool GoTo(int lineIndex, bool isLeftLine = false)
            => DiffView.GoTo(lineIndex, isLeftLine);

        /// <summary>
        /// Goes to a specific line.
        /// </summary>
        /// <param name="line">The line to go to.</param>
        /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
        /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
        public bool GoTo(DiffPiece line, bool isLeftLine = false)
            => DiffView.GoTo(line, isLeftLine);

        /// <summary>
        /// Gets the line diff information.
        /// </summary>
        /// <param name="lineIndex">The index of the line to get information.</param>
        /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
        /// <returns>The line diff information instance; or null, if non-exists.</returns>
        public DiffPiece GetLine(int lineIndex, bool isLeftLine = false)
            => DiffView.GetLine(lineIndex, isLeftLine);

        /// <summary>
        /// Sets the style to the menu buttons.
        /// The buttons in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="style">The button style to set.</param>
        public void SetMenuButtonStyle(Style style)
            => DiffView.SetMenuButtonStyle(style);

        /// <summary>
        /// Sets the control template to the menu buttons.
        /// The buttons in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="template">The control template to set.</param>
        public void SetMenuButtonTemplate(ControlTemplate template)
            => DiffView.SetMenuButtonTemplate(template);

        /// <summary>
        /// Sets the style to the menu text input boxes.
        /// The text input boxes in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="style">The button style to set.</param>
        public void SetMenuTextBoxStyle(Style style)
            => DiffView.SetMenuTextBoxStyle(style);

        /// <summary>
        /// Sets the control template to the menu text input boxes.
        /// The text input boxes in customized menu bar will not be impacted.
        /// </summary>
        /// <param name="template">The control template to set.</param>
        public void SetMenuTextBoxTemlate(ControlTemplate template)
            => DiffView.SetMenuTextBoxTemlate(template);
    }
}
