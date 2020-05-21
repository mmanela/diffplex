using System.Collections.Generic;

namespace DiffPlex.DiffBuilder.Model
{
    public class DiffPaneModel
    {
        public List<DiffPiece> Lines { get; }
        
        public bool? HasDifferences { get; internal set; }

        public DiffPaneModel()
        {
            Lines = new List<DiffPiece>();
        }
    }
}