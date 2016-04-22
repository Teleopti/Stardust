<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>
<%@ Import Namespace="AuthBridge.Configuration" %>
<%@ Import Namespace="Microsoft.IdentityModel.Configuration" %>


<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        var ex = ViewData.Model.Exception;
        if (Request.IsLocal)
            exTrace.Visible = true;
        exMessage.Text = ex.Message;
        exTrace.Text = ex.StackTrace;

        returnToScopeApplication(ex);
    }

    private void returnToScopeApplication(Exception exception)
    {
        var configuration = ConfigurationManager.GetSection("authBridge/multiProtocolIssuer") as MultiProtocolIssuerSection;
        var microsoftIdentityModelSection = ConfigurationManager.GetSection("microsoft.identityModel") as MicrosoftIdentityModelSection;
        var service = microsoftIdentityModelSection.ServiceElements.OfType<ServiceElement>().FirstOrDefault();
        var wsFederation = service.FederatedAuthentication.WSFederation;
        if (wsFederation != null)
        {
            clearFederationContext();
            if (exception.StackTrace.Contains("AzureAdOAuthHandler"))
            {
                var tokenEndpoint= configuration.ClaimProviders["urn:AzureAd"].Params["tokenEndpoint"].Value;
                var signoutUrl = tokenEndpoint.Replace("token", "logout") + "?post_logout_redirect_uri=" + HttpUtility.UrlEncode(wsFederation.SignOutReply);
                Response.AppendHeader("Refresh", "5;url=" + signoutUrl);
                exMessage.Text = "You don't have permission, you will be signed out in 5 seconds, please sign in using another account.";
            }
            else
            {
                Response.Redirect(wsFederation.Issuer + "?wa=wsignin1.0&wtrealm=" + wsFederation.Realm + "&wctx=ru%3d" + wsFederation.SignOutReply, true);
            }
        }
    }

    private void clearFederationContext()
    {
        if (Request.Cookies["FederationContext"] != null)
        {
            HttpCookie myCookie = new HttpCookie("FederationContext");
            myCookie.Expires = DateTime.Now.AddDays(-1d);
            Response.Cookies.Add(myCookie);
        }
    }

</script>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Error
</asp:Content>

<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
	<div class="app-title">Teleopti WFM</div>
	<div class="alert alert-danger" style="max-width: 300px; margin-right: auto;margin-left: auto;" role="alert">
		<p><asp:Label ID="exMessage" runat="server" Font-Bold="true" Font-Size="Large" /></p>
	</div>
	<div>
		<asp:Label ID="exTrace" runat="server" Visible="false" />
	</div>
</asp:Content>
