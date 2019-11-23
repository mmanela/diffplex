namespace DiffPlex.Chunkers
{
    public class WordChunker:DelimiterChunker
    {
        private static char[] WordSeparaters { get; } = { ' ', '\t', '.', '(', ')', '{', '}', ',', '!' };

        public WordChunker() : base(WordSeparaters)
        {
        }
    }
}