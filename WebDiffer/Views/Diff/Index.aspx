<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage"  %>

<asp:Content ID="indexContent" ContentPlaceHolderID="body" runat="server">
    <div id="header">
        <h1>
            DiffPlex Web</h1>
    </div>
    <h2>Create a diff</h2>
    <% Html.BeginRouteForm("Diff", FormMethod.Post, new object { }); %>
  
  
      <div id="oldTextInput" class="formField">
        <div>
            <label for="oldText">
                Old Text</label></div>
        <div>
            <%=Html.TextArea("oldText", new { rows=20, cols=70 }) %></div>
    </div>
     <div id="newTextInput" class="formField">
        <div>
            <label for="newText">
                New Text</label></div>
        <div>
            <%=Html.TextArea("newText", new { rows = 20, cols = 70 })%></div>
    </div>


    <div class="clear"></div>
    <div class="formField">
        <input type="submit" name="diff" value="Create Diff" />
    </div>
    <% Html.EndForm(); %>
</asp:Content>
