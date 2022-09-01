using DiffPlex.DiffBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Trivial.Text;
using Trivial.Web;

namespace DiffPlex.UI;

/// <summary>
/// The local web app command handler for diff text.
/// </summary>
public class DiffTextLocalWebAppCommandHandler : ILocalWebAppCommandHandler
{
    /// <summary>
    /// Initializes a new instance of the DiffTextLocalWebAppCommandHandler class.
    /// </summary>
    public DiffTextLocalWebAppCommandHandler()
    {
        try
        {
            Version = Assembly.GetCallingAssembly()?.GetName()?.Version?.ToString();
        }
        catch (InvalidOperationException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (MemberAccessException)
        {
        }

        if (string.IsNullOrWhiteSpace(Version)) Version = "1.0.0.0";
    }

    /// <summary>
    /// Gets or sets the description of the command handler.
    /// </summary>
    public string Description { get; set; } = "The diff text.";

    /// <summary>
    /// Gets or sets the version of the command handler.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="manifest">The manifest of the local web app.</param>
    /// <returns>The response message.</returns>
    public async Task<LocalWebAppResponseMessage> Process(LocalWebAppRequestMessage request, LocalWebAppManifest manifest)
    {
        var oldText = request?.Data?.TryGetStringValue("old") ?? request?.Data?.TryGetStringValue("o") ?? string.Empty;
        var newText = request?.Data?.TryGetStringValue("new") ?? request?.Data?.TryGetStringValue("n") ?? string.Empty;
        var diff = SideBySideDiffBuilder.Diff(oldText, newText, true, false);
        var json = await Task.FromResult(JsonObjectNode.ConvertFrom(diff));
        return new(json);
    }
}
