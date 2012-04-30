using System;
using System.Globalization;

namespace Teleopti.Analytics.Portal
{
	public partial class PmContainer : MatrixBasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ContainerControl.Attributes.Add("src", string.Format(CultureInfo.InvariantCulture, "PerformanceManager/Default.aspx{0}", QueryStringWithPrefix));
		}
	}
}