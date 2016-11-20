<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<DiffPiece>" %>

<% if (!string.IsNullOrEmpty(Model.Text))
   {
       string spaceValue = "\u00B7";
       string tabValue = "\u00B7\u00B7";
       if (Model.Type == ChangeType.Deleted || Model.Type == ChangeType.Inserted || Model.Type == ChangeType.Unchanged)
       {
           %><%= Html.Encode(Model.Text).Replace(" ", spaceValue).Replace("\t", tabValue) %><%
       }
       else if (Model.Type == ChangeType.Modified)
       {
           foreach (var character in Model.SubPieces)
           {
               if (character.Type == ChangeType.Imaginary) continue;
               %><span class="<%= character.Type.ToString() %>Character"><%= Html.Encode(character.Text.Replace(" ", spaceValue.ToString())) %></span><%
           }
       }

   }
   %>
