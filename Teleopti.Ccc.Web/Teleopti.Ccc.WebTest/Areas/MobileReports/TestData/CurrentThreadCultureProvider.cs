namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	using System.Globalization;

	using Teleopti.Ccc.Web.Core.RequestContext;

	public class CurrentThreadCultureProvider : ICultureProvider
	{
		#region ICultureProvider Members

		public CultureInfo GetCulture()
		{
			return CultureInfo.CurrentCulture;
		}

		#endregion
	}
}