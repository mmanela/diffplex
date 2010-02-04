namespace DiffPlex.TextDiffer.Model
{
    public class DiffModel
    {
        public DiffPaneModel OldText { get; private set; }
        public DiffPaneModel NewText { get; private set; }

        public DiffModel()
        {
            OldText = new DiffPaneModel();
            NewText = new DiffPaneModel();
        }
    }
}