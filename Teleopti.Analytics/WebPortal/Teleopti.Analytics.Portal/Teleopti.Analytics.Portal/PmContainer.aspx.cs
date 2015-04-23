using System;
using System.Globalization;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal
{
	public partial class PmContainer : Page
	{
		private void Page_Init(object sender, EventArgs e)
		{
			if (StateHolder.UserObject == null)
			{
				if (StateHolder.UserName == null)
					Response.Redirect(LoginUrl());
			}

			if (StateHolder.DoForceFormsLogOn)
			{
				if (StateHolder.UserName == null)
				{
					var sec = (AuthenticationSection)HttpContext.Current.GetSection("system.web/authentication");
					if (sec.Mode == AuthenticationMode.Windows && !Page.IsPostBack)
						Response.Redirect(LoginUrl());
				}
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			ContainerControl.Attributes.Add("src", string.Format(CultureInfo.InvariantCulture, "PerformanceManager/Default.aspx{0}", QueryStringWithPrefix));
		}

		
		protected string LoginUrl()
		{
			return string.Format("~/Login.aspx{0}", QueryStringWithPrefix);
		}

		protected virtual string QueryStringWithPrefix
		{
			get { return string.Concat("?", Request.QueryString.ToString()); }
		}
	}
}