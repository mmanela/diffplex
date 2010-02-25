using System.Collections.Generic;

namespace DiffPlex.DiffBuilder.Model
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