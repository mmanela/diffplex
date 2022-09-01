using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DiffPlex.WindowsForms.Controls;

using TestData = DiffPlex.Model.TestData;

namespace DiffPlex.WindowsForms
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// The diff viewer control.
        /// </summary>
        private DiffViewer diffView;

        /// <summary>
        /// Initializes a new instance of the Form2 class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var isDark = now.Hour < 6 || now.Hour >= 18;
            BackColor = isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 251, 251, 251);
            diffView = new DiffViewer
            {
                Margin = new Padding(0),
                Dock = DockStyle.Fill,
                ForeColor = isDark ? Color.FromArgb(255, 240, 240, 240) : Color.FromArgb(255, 32, 32, 32),
                OldText = TestData.DuplicateText(TestData.OldText, 20),
                NewText = TestData.DuplicateText(TestData.NewText, 20)
            };
            MainLayoutPanel.Controls.Add(diffView, 0, 0);
            diffView.SetHeaderAsOldToNew();
        }

        private void SwitchButton_Click(object sender, EventArgs e)
        {
            if (diffView.IsInlineViewMode)
            {
                diffView.ShowSideBySide();
                return;
            }

            diffView.ShowInline();
        }

        private void FutherActionsButton_Click(object sender, EventArgs e)
        {
            diffView.OpenViewModeContextMenu();
        }
    }
}
