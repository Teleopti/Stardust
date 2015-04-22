using System;
using System.Globalization;
using System.Web.UI;

namespace Teleopti.Analytics.Portal
{
	public partial class PmContainer : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ContainerControl.Attributes.Add("src", string.Format(CultureInfo.InvariantCulture, "PerformanceManager/Default.aspx{0}", QueryStringWithPrefix));
		}

		protected virtual string QueryStringWithPrefix
		{
			get { return string.Concat("?", Request.QueryString.ToString()); }
		}
	}
}