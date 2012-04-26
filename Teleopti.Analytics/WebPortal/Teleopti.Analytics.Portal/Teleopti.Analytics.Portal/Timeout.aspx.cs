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
			setTexts();
		}

		private void setTexts()
		{
			//labelHeader.Text=Resources.XXX;
			//labelText.Text=Resources.XXX;
		}
	}
}