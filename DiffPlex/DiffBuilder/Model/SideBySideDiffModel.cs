namespace DiffPlex.DiffBuilder.Model
{
    public class SideBySideDiffModel
    {
        public DiffPaneModel OldText { get; private set; }
        public DiffPaneModel NewText { get; private set; }

        public SideBySideDiffModel()
        {
            OldText = new DiffPaneModel();
            NewText = new DiffPaneModel();
        }
    }
}