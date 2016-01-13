<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<SideBySideDiffModel>" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="body" runat="server">
       <div id="diffBox">
        <div id="leftPane">
             <div class="diffHeader">Old Text</div>
             <% Html.RenderPartial("DiffPane", Model.OldText); %>
        </div>
        <div id="rightPane">
           <div class="diffHeader">New Text</div>
           <% Html.RenderPartial("DiffPane", Model.NewText); %>
        </div>
        <div class="clear">
        </div>
    </div>

    <script type="text/javascript">

        $(function() {
            InitializeDiffPanes();
        });
    </script>
</asp:Content>