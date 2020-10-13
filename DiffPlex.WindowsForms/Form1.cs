using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.WindowsForms.Extensions;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DiffPlex.WindowsForms
{
	public partial class Form1 : Form
	{
		private const string OldText =
			@"We the people
of the united states of america
establish justice
ensure domestic tranquility
provide for the common defence
secure the blessing of liberty
to ourselves and our posterity
";

		private const string NewText =
			@"We the people
in order to form a more perfect union
establish justice
ensure domestic tranquility
promote the general welfare and
secure the blessing of liberty
to ourselves and our posterity
do ordain and establish this constitution
for the United States of America




";

		public Form1()
		{
			InitializeComponent();

			PerformInlineDiff();
			PerformSideBySideDiff();
		}

		/// <summary>
		/// Populate the top rich text box with an inline compare.
		/// </summary>
		private void PerformInlineDiff()
		{
			var builder = InlineDiffBuilder.Diff(OldText, NewText);
			if (builder.HasDifferences)
			{
				foreach (var line in builder.Lines)
				{
					if (!string.IsNullOrWhiteSpace(richTextBoxInline.Text))
					{
						richTextBoxInline.AppendText(Environment.NewLine);
					}
					switch (line.Type)
					{
						case ChangeType.Unchanged:
							richTextBoxInline.AppendText(line.Text);
							break;
						case ChangeType.Deleted:
							richTextBoxInline.AppendText(line.Text, Color.Red);
							break;
						case ChangeType.Inserted:
							richTextBoxInline.AppendText(line.Text, Color.Blue);
							break;
						case ChangeType.Imaginary:
							richTextBoxInline.AppendText(String.Empty);
							break;
						case ChangeType.Modified:
							richTextBoxInline.AppendText(line.Text, Color.Green);
							break;
						default:
							richTextBoxInline.AppendText(line.Text, Color.Purple);
							break;
					}
				}
			}
			else
			{
				richTextBoxInline.AppendText(NewText);
			}
		}

		/// <summary>
		/// Populate the bottom left and right rich text box with a side-by-side compare.
		/// </summary>
		private void PerformSideBySideDiff()
		{
			var builder = SideBySideDiffBuilder.Diff(OldText, NewText);
			if (builder.OldText.HasDifferences || builder.NewText.HasDifferences)
			{
				foreach (var line in builder.OldText.Lines)
				{
					if (!string.IsNullOrWhiteSpace(richTextBoxSideLeft.Text))
					{
						richTextBoxSideLeft.AppendText(Environment.NewLine);
					}
					if (line.SubPieces.Any())
					{
						foreach (var piece in line.SubPieces)
						{
							AddText(richTextBoxSideLeft, piece);
						}
					}
					else
					{
						AddText(richTextBoxSideLeft, line);
					}
				}

				foreach (var line in builder.NewText.Lines)
				{
					if (!string.IsNullOrWhiteSpace(richTextBoxSideRight.Text))
					{
						richTextBoxSideRight.AppendText(Environment.NewLine);
					}
					if (line.SubPieces.Any())
					{
						foreach (var piece in line.SubPieces)
						{
							AddText(richTextBoxSideRight, piece);
						}
					}
					else
					{
						AddText(richTextBoxSideRight, line);
					}
				}
			}
			else
			{
				richTextBoxSideLeft.AppendText(OldText);
				richTextBoxSideRight.AppendText(NewText);
			}
		}

		/// <summary>
		/// Add text to a rich text box, styling items as needed.
		/// </summary>
		/// <param name="box">Rich Text Box to add the text to.</param>
		/// <param name="piece">Piece of content. Could be an entire line if it's unchanged, or a subpiece in a line.</param>
		private void AddText(RichTextBox box, DiffPiece piece)
		{
			switch (piece.Type)
			{
				case ChangeType.Unchanged:
					box.AppendText(piece.Text);
					break;
				case ChangeType.Deleted:
					box.AppendText(piece.Text, Color.Red);
					break;
				case ChangeType.Inserted:
					box.AppendText(piece.Text, Color.Blue);
					break;
				case ChangeType.Imaginary:
					box.AppendText(String.Empty);
					break;
				case ChangeType.Modified:
					box.AppendText(piece.Text, Color.Green);
					break;
				default:
					box.AppendText(piece.Text, Color.Purple);
					break;
			}
		}
	}
}
