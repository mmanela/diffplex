namespace SilverlightDiffer
{
    public class FontInfo
    {
        public FontInfo(string family, double linePadding)
        {
            FontFamily = family;
            LinePadding = linePadding;
            IsMonoSpaced = false;
        }
        public FontInfo(string family, double linePadding, double characterWidth, double leftOffset)
        {
            FontFamily = family;
            LinePadding = linePadding;
            CharacterWidth = characterWidth;
            LeftOffset = leftOffset;
            IsMonoSpaced = true;
        }

        public string FontFamily { get; set; }
        public double LinePadding { get; set; }
        public bool IsMonoSpaced { get; set; }
        public double CharacterWidth { get; set; }
        public double LeftOffset { get; set; }
    }
}