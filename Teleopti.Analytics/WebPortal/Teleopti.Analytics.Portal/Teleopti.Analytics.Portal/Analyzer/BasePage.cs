using System;
using System.Data;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

using Teleopti.Analytics.Portal.AnalyzerAPI;

public enum AuthenticationType
{
    Anonymous,
    Basic,
    Integrated
}

public class BasePage : Page
{
     Teleopti.Analytics.Portal.Properties.Settings set = new Teleopti.Analytics.Portal.Properties.Settings();
     private int _reportId;

     void Page_Init(object sender, EventArgs e)
     {
         if (!string.IsNullOrEmpty((string)Request.QueryString.Get("REPORTID")))
         {
             _reportId = int.Parse(Request.QueryString["REPORTID"], CultureInfo.CurrentCulture);

         }
     }

    #region Class properties
    
    protected int ReportId
     {
         get { return this._reportId; }
     }

    protected string MachineName
    {
        get { return set.MachineName;}
    }

    protected string VirtualDir
    {
        get { return set.VirtualDir; }
    }

    protected SecurityContext CurrentContext
    {
        get { return (SecurityContext)Session["__currentContext"]; }
        set { Session["__currentContext"] = value; }
    }

    protected AuthenticationType AuthType
    {
        get { return (AuthenticationType) set.Authentication; }
    }

    protected string Message
    {
        get
        {
            Label label = (Label)FindControl("lblMessage");
            if (label != null)
            {
                return label.Text;
            }
            return string.Empty;
        }
        set
        {
            Label label = (Label)FindControl("lblMessage");
            if (label != null)
            {
                label.Text = value;
            }
        }
    }

    #endregion

    #region Class methods

    protected Analyzer2005 GetProxy()
    {
        Analyzer2005 az = new Analyzer2005();
        az.Url = string.Format(
            "http://{0}/{1}/services/analyzer2005.asmx",
            MachineName,
            VirtualDir
        );

        if (AuthType == AuthenticationType.Basic)
        {
            string userId = Request.ServerVariables["AUTH_USER"];
            string password = Request.ServerVariables["AUTH_PASSWORD"];
            CredentialCache cc = new CredentialCache();
            cc.Add(
                new Uri(az.Url),
                "Basic",
                new NetworkCredential(userId, password)
            );
            az.PreAuthenticate = true;
            az.Credentials = cc;
        }
        else if (AuthType == AuthenticationType.Integrated)
        {
            az.PreAuthenticate = true;
            az.Credentials = CredentialCache.DefaultCredentials;
        }

        CookieContainer cookies = new CookieContainer();
        cookies.Add(new Cookie("ASP.NET_SessionId", Session.SessionID, "/", MachineName));
        az.CookieContainer = cookies;
        return az;
    }

    protected void DisposeProxy(Analyzer2005 az)
    {
        if (az != null)
        {
            az.Dispose();
        }
    }

    protected void CheckState(BaseState state)
    {
        if (!state.Success)
        {
            throw new Exception(state.Message);
        }
    }

    protected void LogonUser(Analyzer2005 az, string userId, string password)
    {
        if (CurrentContext == null)
        {
            SecurityContext context = az.LogonUser(".", userId, password, Session.Timeout);
            CheckState(context);
            CurrentContext = context;
        }
    }

    protected void LogoffUser(Analyzer2005 az)
    {
        if (CurrentContext != null)
        {
            CheckState(az.LogoffUser(CurrentContext));
            CurrentContext = null;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Message = string.Empty;
    }

    #endregion
}
