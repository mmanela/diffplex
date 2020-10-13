using System.Drawing;
using System.Windows.Forms;

namespace DiffPlex.WindowsForms.Extensions
{
	/// <summary>
	/// Extension methods for the RichTextBox control.
	/// </summary>
	public static class RichTextBoxExtensions
	{
		/// <summary>
		/// Add text to a RichTextBox with a specific color.
		/// From https://stackoverflow.com/a/1926822/11912.
		/// </summary>
		/// <param name="box"></param>
		/// <param name="text"></param>
		/// <param name="color"></param>
		public static void AppendText(this RichTextBox box, string text, Color color)
		{
			box.SelectionStart = box.TextLength;
			box.SelectionLength = 0;

			box.SelectionColor = color;
			box.AppendText(text);
			box.SelectionColor = box.ForeColor;
		}
	}
}
