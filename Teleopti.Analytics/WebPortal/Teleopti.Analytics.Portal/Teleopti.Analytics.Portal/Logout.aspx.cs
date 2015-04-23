using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal
{
	public partial class Logout : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			FormsAuthentication.SignOut();
			StateHolder.UserObject = null;
			HttpContext.Current.Session["FORCEFORMSLOGIN"] = true;
			HttpContext.Current.Session["LOGOUT"] = true;
			ID = HttpContext.Current.Session.SessionID;
			Response.Redirect(LogOnUrl());
		}
		protected string LogOnUrl()
		{
			return string.Format("~/Login.aspx{0}", QueryStringWithPrefix);
		}

		protected virtual string QueryStringWithPrefix
		{
			get { return string.Concat("?", Request.QueryString.ToString()); }
		}
	}
}