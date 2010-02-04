using System.Collections.Generic;

namespace DiffPlex.Model
{
    public class DiffResult
    {
        public string[] PiecesOld { get; private set; }

        public string[] PiecesNew { get; private set; }

        public IList<DiffBlock> DiffBlocks { get; private set; }

        public DiffResult(string[] peicesOld, string[] piecesNew, IList<DiffBlock> blocks)
        {
            PiecesOld = peicesOld;
            PiecesNew = piecesNew;
            DiffBlocks = blocks;
        }
    }
}