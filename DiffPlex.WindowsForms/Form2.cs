using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiffPlex.WinForms.Demo
{
    public partial class Form2 : Form
    {
        /// <summary>
        /// Initializes a new instance of the Form2 class.
        /// </summary>
        public Form2()
        {
            InitializeComponent();

            DiffView = new Wpf.Controls.DiffViewer();
            diffViewHost.Child = DiffView;
            var isDark = Wpf.Demo.TestData.FillDiffViewer(DiffView, 20);
            BackColor = isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 251, 251, 251);
        }

        /// <summary>
        /// Gets the diff viewer control.
        /// </summary>
        public Wpf.Controls.DiffViewer DiffView { get; }
    }
}
