namespace DiffPlex.WindowsForms
{
	partial class Form1
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
			this.groupBoxInline = new System.Windows.Forms.GroupBox();
			this.richTextBoxInline = new System.Windows.Forms.RichTextBox();
			this.groupBoxSideBySide = new System.Windows.Forms.GroupBox();
			this.richTextBoxSideRight = new System.Windows.Forms.RichTextBox();
			this.richTextBoxSideLeft = new System.Windows.Forms.RichTextBox();
			this.groupBoxInline.SuspendLayout();
			this.groupBoxSideBySide.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxInline
			// 
			this.groupBoxInline.Controls.Add(this.richTextBoxInline);
			this.groupBoxInline.Location = new System.Drawing.Point(12, 12);
			this.groupBoxInline.Name = "groupBoxInline";
			this.groupBoxInline.Size = new System.Drawing.Size(695, 241);
			this.groupBoxInline.TabIndex = 0;
			this.groupBoxInline.TabStop = false;
			this.groupBoxInline.Text = "Inline Example";
			// 
			// richTextBoxInline
			// 
			this.richTextBoxInline.Location = new System.Drawing.Point(6, 19);
			this.richTextBoxInline.Name = "richTextBoxInline";
			this.richTextBoxInline.Size = new System.Drawing.Size(683, 216);
			this.richTextBoxInline.TabIndex = 0;
			this.richTextBoxInline.Text = "";
			// 
			// groupBoxSideBySide
			// 
			this.groupBoxSideBySide.Controls.Add(this.richTextBoxSideRight);
			this.groupBoxSideBySide.Controls.Add(this.richTextBoxSideLeft);
			this.groupBoxSideBySide.Location = new System.Drawing.Point(12, 259);
			this.groupBoxSideBySide.Name = "groupBoxSideBySide";
			this.groupBoxSideBySide.Size = new System.Drawing.Size(695, 244);
			this.groupBoxSideBySide.TabIndex = 1;
			this.groupBoxSideBySide.TabStop = false;
			this.groupBoxSideBySide.Text = "Side-By-Side Example";
			// 
			// richTextBoxSideRight
			// 
			this.richTextBoxSideRight.Location = new System.Drawing.Point(359, 19);
			this.richTextBoxSideRight.Name = "richTextBoxSideRight";
			this.richTextBoxSideRight.Size = new System.Drawing.Size(330, 204);
			this.richTextBoxSideRight.TabIndex = 1;
			this.richTextBoxSideRight.Text = "";
			// 
			// richTextBoxSideLeft
			// 
			this.richTextBoxSideLeft.Location = new System.Drawing.Point(6, 19);
			this.richTextBoxSideLeft.Name = "richTextBoxSideLeft";
			this.richTextBoxSideLeft.Size = new System.Drawing.Size(330, 204);
			this.richTextBoxSideLeft.TabIndex = 0;
			this.richTextBoxSideLeft.Text = "";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(719, 533);
			this.Controls.Add(this.groupBoxSideBySide);
			this.Controls.Add(this.groupBoxInline);
			this.Name = "Form1";
			this.Text = "DiffPlex Windows Forms Demo";
			this.groupBoxInline.ResumeLayout(false);
			this.groupBoxSideBySide.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBoxInline;
		private System.Windows.Forms.GroupBox groupBoxSideBySide;
		private System.Windows.Forms.RichTextBox richTextBoxInline;
		private System.Windows.Forms.RichTextBox richTextBoxSideRight;
		private System.Windows.Forms.RichTextBox richTextBoxSideLeft;
	}
}

