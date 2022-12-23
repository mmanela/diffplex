using System.Collections.Generic;

namespace DiffPlex.Chunkers
{
    public class LineEndingsPreservingChunker:IChunker
    {
#if NETSTANDARD1_3_OR_GREATER || NET46_OR_GREATER || NETCOREAPP
        private static readonly string[] emptyStringArray = System.Array.Empty<string>();
#else
        private static readonly string[] emptyStringArray = new string[0];
#endif

        /// <summary>
        /// Gets the default singleton instance of the chunker.
        /// </summary>
        public static LineEndingsPreservingChunker Instance { get; } = new LineEndingsPreservingChunker();

        public string[] Chunk(string text)
        {
            if (string.IsNullOrEmpty(text))
                return emptyStringArray;

            var output = new List<string>();
            var lastCut = 0;
            for (var currentPosition = 0; currentPosition < text.Length; currentPosition++)
            {
                char ch = text[currentPosition];
                switch (ch)
                {
                    case '\n':
                    case '\r':
                        currentPosition++;
                        if (ch == '\r' && currentPosition < text.Length && text[currentPosition] == '\n')
                        {
                            currentPosition++;
                        }
                        var str = text.Substring(lastCut, currentPosition - lastCut);
                        lastCut = currentPosition;
                        output.Add(str);
                        break;
                    default:
                        continue;
                }
            }

            if (lastCut != text.Length)
            {
                var str = text.Substring(lastCut, text.Length - lastCut);
                output.Add(str);
            }

            return output.ToArray();
        }
    }
}