<%@ Page Language="C#" MasterPageFile="ErrorMasterPage.master" %>

<asp:Content runat="server" ContentPlaceHolderID="HeaderContentPlaceHolder">
	<% Response.StatusCode = 404; %>
	<title>Page not found</title>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder">
	<div class="content center">
		<h1>404</h1>
		<p>Oops! That page does not exist.</p>
		<input action="action" onclick="window.history.go(-1); return false;" type="button" class="btn" value="Back" />
	</div>
</asp:Content>