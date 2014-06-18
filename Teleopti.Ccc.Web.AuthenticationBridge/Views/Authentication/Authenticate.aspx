<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<AuthBridge.Web.Controllers.ProviderViewModel[]>" %>

<%@ Import Namespace="Teleopti.Ccc.UserTexts" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Teleopti Authentication Bridge
</asp:Content>
<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
	<div><h1 style="color: #003366; font-family: Calibri;">Teleopti WFM</h1></div>
    <div id="selector" class="login-form">
        <form action="" method="get">
            <input type="hidden" name="action" value="verify" />
            <fieldset>
                <div class="panel panel-info">
                    <div class="panel-heading">
                        <span><%=Resources.SignInWith %></span>
                    </div>
                    <div id="buttons" class="list-group">
	                    <% foreach (var provider in Model)
						   { %>
						<a class="<%= provider.Identifier.Replace("urn:","").ToLowerInvariant() %> list-group-item"
							href="authenticate?whr=<%= provider.Identifier %>" title="<%= provider.DisplayName %>"><%= provider.DisplayName %></a>
						   <% }
						%>
                    </div>
                </div>
            </fieldset>
            <input type="hidden" value="<%=HttpContext.Current.Request.QueryString["ReturnUrl"] %>" />
        </form>
    </div>
</asp:Content>
<asp:Content ID="pageSpecificScripts" ContentPlaceHolderID="PageSpecificScripts"
    runat="server">
</asp:Content>
