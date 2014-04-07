<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="Teleopti.Ccc.UserTexts" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Teleopti Authentication Bridge
</asp:Content>
<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
    <img class="img-responsive login-logo" src="/Content/Images/mobile_logo.png" />
    <div id="selector" class="login-form">
        <form action="" method="get">
            <input type="hidden" name="action" value="verify" />
            <fieldset>
                <h3>
                    <%--<span class="label label-default"><%=Resources.ResourceManager.GetString("SignInWith") %></span>--%>
                </h3>
                <div class="panel panel-info">
                    <div class="panel-heading">
                        <span><%=Resources.SignInWith %></span>
                    </div>
                    <div id="buttons" class="list-group">
                        <% if (ViewData["Windows"] != null && (bool)ViewData["Windows"])
         {    %>
                            <a class="windows list-group-item"
                                href="authenticate?whr=urn:Windows" title="Windows">Windows</a>
                        <% } %>
                        <% if (ViewData["Teleopti"] != null && (bool)ViewData["Teleopti"])
         {    %>
                        <a class="teleopti list-group-item"
                            href="authenticate?whr=urn:Teleopti" title="Teleopti">Teleopti</a>
                        <% } %>
                        <% if (ViewData["Yahoo"] != null && (bool)ViewData["Yahoo"])
         { %>
                        <a class="yahoo list-group-item"
                            href="authenticate?whr=urn:Yahoo" title="Yahoo">Yahoo</a>
                        <% } %>

                        <% if (ViewData["Google"] != null && (bool)ViewData["Google"])
         { %>
                        <a class="google list-group-item"
                            href="authenticate?whr=urn:Google" title="Google">Google</a>
                        <% } %>
                        <% if (ViewData["WindowsLive"] != null && (bool)ViewData["WindowsLive"])
         { %>
                        <a class="liveid list-group-item"
                            href="authenticate?whr=urn:LiveId" title="Windows Live">Windows Live</a>
                        <% } %>
                        <% if (ViewData["Facebook"] != null && (bool)ViewData["Facebook"])
         { %>
                        <a class="facebook list-group-item"
                            href="authenticate?whr=urn:Facebook" title="Facebook">Facebook</a>
                        <% } %>
                        <% if (ViewData["Twitter"] != null && (bool)ViewData["Twitter"])
         { %>
                        <a class="twitter list-group-item"
                            href="authenticate?whr=urn:Twitter" title="Twitter">Twitter</a>
                        <% } %>
                        <% if (ViewData["IdentityServer"] != null && (bool)ViewData["IdentityServer"])
         { %>
                        <a class="list-group-item"
                            href="authenticate?whr=urn:IdentityServer" title="IdentityServer">Identity Server (WS-Fed + SAML)</a>
                        <% } %>
                        <% if (ViewData["WindowsAzureAD"] != null && (bool)ViewData["WindowsAzureAD"])
         { %>
                        <a class="list-group-item"
                            href="authenticate?whr=urn:office365:auth10preview" title="WindowsAzure AD">Windows Azure Active Directory (Office 365)</a>
                        <% } %>
                        <% if (ViewData["SalesForce"] != null && (bool)ViewData["SalesForce"])
         { %>
                        <a class="salesforce list-group-item"
                            href="authenticate?whr=urn:SalesForce" title="SalesForce">SalesForce</a>
                        <% } %>
                        <% if (ViewData["MyOpenId"] != null && (bool)ViewData["MyOpenId"])
         { %>
                        <a class="myopenid list-group-item"
                            href="authenticate?whr=urn:MyOpenId" title="MyOpenId">MyOpenId</a>
                        <% } %>
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
