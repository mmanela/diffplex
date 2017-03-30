using System.Collections.Generic;

namespace DiffPlex.DiffBuilder.Model
{
    public enum ChangeType
    {
        Unchanged,
        Deleted,
        Inserted,
        Imaginary,
        Modified
    }

    public class DiffPiece
    {
        public ChangeType Type { get; set; }
        public int? Position { get; set; }
        public string Text { get; set; }
        public List<DiffPiece> SubPieces { get; set; } = new List<DiffPiece>();

        public DiffPiece(string text, ChangeType type, int? position = null)
        {
            Text = text;
            Position = position;
            Type = type;
        }

        public DiffPiece()
            : this(null, ChangeType.Imaginary)
        {
        }
    }
}