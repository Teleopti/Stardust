﻿<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<AuthBridge.Web.Controllers.HrdViewModel>" %>

<%@ Import Namespace="Teleopti.Ccc.UserTexts" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Teleopti Authentication Bridge
</asp:Content>
<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
	<div class="con-row" style="margin-top: 10%;">
		<div class="con-flex"></div>
		<div class="con-flex">
			<div style="min-height: 54px;" class="panel material-depth-1 line-center animate-material">
				<div class="sub-header">
					<h1>TELEOPTI WFM</h1>
				</div>
				<div id="spinnerwrapper"></div>
				<div class="con-row animate-content">
					<div class="con-flex">
						<div id="selector">
							<form action="" method="get">
								<input type="hidden" name="action" value="verify" />
								<span><%=Resources.SignInWith %></span>
			
									<% foreach (var provider in Model.Providers)
										{ %>
									<a class="<%= provider.Identifier.Replace("urn:","").ToLowerInvariant() %> list-group-item" href="authenticate?whr=<%= provider.Identifier %>" title="<%= provider.DisplayName %>"><%= provider.DisplayName %></a>
									<% }
									%>
								<input type="hidden" value="<%=HttpContext.Current.Request.QueryString["ReturnUrl"] %>" />
							</form>
						</div>
					</div>
				</div>
			</div>
		</div>
		<div class="con-flex"></div>
	</div>
	<div class="con-row error-row">
		<div class="con-flex">
			<div class="login-message formError">
				<% if (!string.IsNullOrEmpty(Model.ErrorMessage))
					{ %>
				<p><%="Technical information: " + Model.ErrorMessage%></p>
				<% } %>
			</div>
		</div>
	</div>

</asp:Content>
<asp:Content ID="pageSpecificScripts" ContentPlaceHolderID="PageSpecificScripts"
	runat="server">
</asp:Content>
