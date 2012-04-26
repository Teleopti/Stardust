using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using Teleopti.Analytics.Portal.PerformanceManager.Helper;

namespace Teleopti.Analytics.Portal
{
	public partial class Timeout : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			
		}

		protected bool IsForceFormsLogOn
		{
			get
			{
				if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["FORCEFORMSLOGIN"]))
					if (Request.QueryString["FORCEFORMSLOGIN"] == "true")
						return true;

				return false;
			}
		}

		private bool IsPmBrowsed
		{
			get
			{
				return !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["PM"]) && (Request.QueryString["PM"] == "1");
			}
		}

		//private string ReturnUrl
		//{
		//    get
		//    {
		//        if (IsPmBrowsed)
		//            return HttpContext.Current.Server.UrlEncode(string.Format(CultureInfo.InvariantCulture, "PmContainer.aspx?{0}", Request.QueryString));

		//        return string.Empty;
		//    }
		//}

		private void ApplicationAuthentication()
		{
			//Response.Redirect(string.Format(CultureInfo.InvariantCulture, "Login.aspx?{0}&timeouturl={1}", Request.QueryString, ReturnUrl), true);
			Response.Redirect(string.Format(CultureInfo.InvariantCulture, "Login.aspx?{0}", Request.QueryString), true);
		}

		private void WindowsAuthentication()
		{
			Response.Redirect(string.Format(CultureInfo.InvariantCulture, "Login.aspx?{0}", Request.QueryString), true);
		}

		protected void linkButtonRedirect_Click(object sender, EventArgs e)
		{
			if (AuthorizationHelper.CheckWindowsAuthentication(IsForceFormsLogOn))
				WindowsAuthentication();
			else
				ApplicationAuthentication();
		}
	}
}