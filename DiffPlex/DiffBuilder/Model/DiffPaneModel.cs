using System.Collections.Generic;

namespace DiffPlex.DiffBuilder.Model
{
    public class DiffPaneModel
    {
        public List<DiffPiece> Lines { get; }

        public bool HasDifferences
        {
            get
            {
                for (int i = 0; i < Lines.Count; i++)
                {
                    if (Lines[i].Type != ChangeType.Unchanged)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public DiffPaneModel()
        {
            Lines = new List<DiffPiece>();
        }
    }
}