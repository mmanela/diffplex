namespace DiffPlex
{
    public interface IChunker
    {
        /// <summary>
        /// Dive text into sub-parts
        /// </summary>
        string[] Chunk(string text);
    }
}