using System.Collections.Generic;

namespace DiffPlex.TextDiffer.Model
{
    public class DiffPaneModel
    {
        public List<DiffPiece> Lines { get; private set; }

        public DiffPaneModel()
        {
            Lines = new List<DiffPiece>();
        }
    }
}