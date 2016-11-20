﻿<%@  Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<SideBySideDiffModel>" %>

<div id="diffBar">
    <table cellpadding="0" cellspacing="0" class="diffBarTable">
        <% for (int i = 0; i < Model.NewText.Lines.Count; i++)
           {
               var leftType = Model.OldText.Lines[i].Type == ChangeType.Modified ? ChangeType.Deleted : Model.OldText.Lines[i].Type;
               var rightType = Model.NewText.Lines[i].Type == ChangeType.Modified ? ChangeType.Inserted : Model.NewText.Lines[i].Type;
               var leftClass = leftType.ToString() + "Line";
               var rightClass = rightType.ToString() + "Line";
        %>
        <tr line="<%= i %>">
            <td class="<%= leftClass %>">
            </td>
            <td class="<%= rightClass %>">
            </td>
        </tr>
        <% } %>
    </table>
</div>
