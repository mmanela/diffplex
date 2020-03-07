﻿using System;

namespace DiffPlex.Chunkers
{
    public class LineChunker:IChunker
    {
        private readonly string[] lineSeparators = new[] {"\r\n", "\r", "\n"};

        /// <summary>
        /// Gets the default singleton instance of the chunker.
        /// </summary>
        public static LineChunker Instance { get; } = new LineChunker();

        public string[] Chunk(string text)
        {
            return text.Split(lineSeparators, StringSplitOptions.None);
        }
    }
}