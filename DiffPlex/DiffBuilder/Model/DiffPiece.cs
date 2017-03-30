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
        public List<DiffPiece> SubPieces { get; set; }

        public DiffPiece(string text, ChangeType type, int? position = null)
        {
            Text = text;
            Position = position;
            Type = type;
            SubPieces = new List<DiffPiece>();
        }

        public DiffPiece()
            : this(null, ChangeType.Imaginary)
        {
        }
    }
}