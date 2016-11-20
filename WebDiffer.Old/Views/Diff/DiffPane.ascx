<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<DiffPaneModel>" %>

<div class="diffPane">
    <table cellpadding="0" cellspacing="0" class="diffTable">
        <% foreach (var diffLine in Model.Lines)
           { 
        %>
        <tr>
            <td class="lineNumber">
                <%= diffLine.Position.HasValue? diffLine.Position.ToString() : "&nbsp;"  %>
            </td>
            <td class="line <%=diffLine.Type.ToString() %>Line">
                <span class="lineText">
                    <% Html.RenderPartial("DiffLine", diffLine); %>
            </td>
        </tr>
        <%} %>
    </table>
</div>