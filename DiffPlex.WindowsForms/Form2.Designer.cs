
namespace DiffPlex.WinForms.Demo
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.diffViewHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // diffViewHost
            // 
            this.diffViewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diffViewHost.Location = new System.Drawing.Point(0, 0);
            this.diffViewHost.Margin = new System.Windows.Forms.Padding(0);
            this.diffViewHost.Name = "diffViewHost";
            this.diffViewHost.Size = new System.Drawing.Size(800, 450);
            this.diffViewHost.TabIndex = 0;
            this.diffViewHost.Child = null;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.diffViewHost);
            this.Name = "Form2";
            this.Text = "Demo - DiffPlex";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost diffViewHost;
    }
}