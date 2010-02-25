using System;
using System.Collections.Generic;
using DiffPlex.Model;

namespace DiffPlex.ConsoleRunner
{
    public static class UnidiffFormater
    {
        private const string NoChangeSymbol = "  ";
        private const string InsertSymbol = "+ ";
        private const string DeleteSymbol = "- ";

        public static List<string> Generate(DiffResult lineDiff)
        {
            var uniLines = new List<string>();
            int bPos = 0;

            foreach (var diffBlock in lineDiff.DiffBlocks)
            {
                for (; bPos < diffBlock.InsertStartB; bPos++)
                    uniLines.Add(NoChangeSymbol + lineDiff.PiecesNew[bPos]);

                int i = 0;
                for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
                {
                    uniLines.Add(DeleteSymbol + lineDiff.PiecesOld[i + diffBlock.DeleteStartA]);
                    uniLines.Add(InsertSymbol + lineDiff.PiecesNew[i + diffBlock.InsertStartB]);
                    bPos++;
                }

                if (diffBlock.DeleteCountA > diffBlock.InsertCountB)
                {
                    for (; i < diffBlock.DeleteCountA; i++)
                        uniLines.Add(DeleteSymbol + lineDiff.PiecesOld[i + diffBlock.DeleteStartA]);
                }
                else
                {
                    for (; i < diffBlock.InsertCountB; i++)
                    {
                        uniLines.Add(InsertSymbol + lineDiff.PiecesNew[i + diffBlock.InsertStartB]);
                        bPos++;
                    }
                }
            }


            return uniLines;
        }
    }
}