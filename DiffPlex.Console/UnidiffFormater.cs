using System;
using System.Collections.Generic;
using DiffPlex.Model;

namespace DiffPlex.CommandLine
{
    public class UnidiffFormater
    {
        private readonly string NoChangeSymbol = "  ";
        private readonly string InsertSymbol = "+ ";
        private readonly string DeleteSymbol = "- ";

        public List<string> Generate(DiffResult lineDiff)
        {
            List<string> uniLines = new List<string>();


            int bPos = 0;

            foreach (var diffBlock in lineDiff.DiffBlocks)
            {
                for (; bPos < diffBlock.InsertStartB; bPos++)
                    uniLines.Add(NoChangeSymbol + lineDiff.PiecesOld[bPos]);

                int i = 0;
                for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
                {
                    uniLines.Add(DeleteSymbol + lineDiff.PiecesNew[i + diffBlock.DeleteStartA]);
                    uniLines.Add(InsertSymbol + lineDiff.PiecesOld[i + diffBlock.InsertStartB]);
                    bPos++;
                }

                if (diffBlock.DeleteCountA > diffBlock.InsertCountB)
                {
                    for (; i < diffBlock.DeleteCountA; i++)
                        uniLines.Add(DeleteSymbol + lineDiff.PiecesNew[i + diffBlock.DeleteStartA]);
                }
                else
                {
                    for (; i < diffBlock.InsertCountB; i++)
                    {
                        uniLines.Add(InsertSymbol + lineDiff.PiecesOld[i + diffBlock.InsertStartB]);
                        bPos++;
                    }
                }
            }


            return uniLines;
        }
    }
}