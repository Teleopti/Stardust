using System;
using System.Globalization;
using Teleopti.Analytics.ReportTexts;

namespace Teleopti.Analytics.Portal
{
	public partial class Timeout : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (Page.IsPostBack)
			{
				Response.Redirect(string.Format(CultureInfo.InvariantCulture, "Login.aspx?{0}", Request.QueryString), true);
			}
			SetTexts();
		}

		private void SetTexts()
		{
			labelHeader.Text=Resources.SessionExpiredHeader;
			labelText.Text=Resources.SessionExpiredText;
			redirectLink.InnerText = Resources.RedirectNow;
		}
	}
}