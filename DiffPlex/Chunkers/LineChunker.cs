using System;

namespace DiffPlex.Chunkers
{
    public class LineChunker:IChunker
    {
        private readonly string[] lineSeparators = new[] {"\r\n", "\r", "\n"};

        public string[] Chunk(string text)
        {
            return text.Split(lineSeparators, StringSplitOptions.None);
        }
    }
}