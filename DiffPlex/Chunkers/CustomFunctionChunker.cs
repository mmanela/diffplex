using System;

namespace DiffPlex.Chunkers
{
    public class CustomFunctionChunker: IChunker
    {
        private readonly Func<string, string[]> customChunkerFunc;

        public CustomFunctionChunker(Func<string, string[]> customChunkerFunc)
        {
            this.customChunkerFunc = customChunkerFunc ?? throw new ArgumentNullException(nameof(customChunkerFunc));
        }

        public string[] Chunk(string text)
        {
            return customChunkerFunc(text);
        }
    }
}