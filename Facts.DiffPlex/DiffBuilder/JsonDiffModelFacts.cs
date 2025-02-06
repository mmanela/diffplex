using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Facts.DiffPlex.DiffBuilder;

/// <summary>
/// The JSON supports test suite of diff models.
/// </summary>
public class JsonDiffModelFacts
{
    /// <summary>
    /// Tests for the JSON serialization supports of diff models.
    /// </summary>
    [Fact]
    public void TestSerialization()
    {
        var t = "The diff model is invalid but does not matter since it is only used for testing.";
        var model = new SideBySideDiffModel(new List<DiffPiece>
        {
            new(),
            new(t, ChangeType.Inserted, 20),
            new("Another test text here…", ChangeType.Deleted, 10),
        }, null);
        var s = JsonSerializer.Serialize(model);
        model = JsonSerializer.Deserialize<SideBySideDiffModel>(s);
        Assert.NotNull(model);
        Assert.Equal(3, model.OldText.Count);
        Assert.Null(model.OldText.Lines[0].Text);
        Assert.Equal(t, model.OldText.Lines[1].Text);
        Assert.Equal(ChangeType.Inserted, model.OldText.Lines[1].Type);
        Assert.Equal(ChangeType.Deleted, model.OldText.Lines[2].Type);
        Assert.Equal(0, model.NewText.Count);
        s = JsonSerializer.Serialize(model.OldText);
        model = new(JsonSerializer.Deserialize<DiffPaneModel>(s), null);
        Assert.Equal(3, model.OldText.Count);
        Assert.Null(model.OldText.Lines[0].Text);
        Assert.Equal(t, model.OldText.Lines[1].Text);
        Assert.Equal(ChangeType.Inserted, model.OldText.Lines[1].Type);
        Assert.Equal(ChangeType.Deleted, model.OldText.Lines[2].Type);
        Assert.Equal(0, model.NewText.Count);
        s = model.OldText.Join();
        Assert.NotNull(s);
    }
}
