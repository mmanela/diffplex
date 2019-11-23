namespace DiffPlex.Chunkers
{
    public class CharacterChunker:IChunker
    {
        public string[] Chunk(string text)
        {
            var s = new string[text.Length];
            for (int i = 0; i < text.Length; i++) s[i] = text[i].ToString();
            return s;
        }
    }
}