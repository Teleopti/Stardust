using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	using System.Globalization;

	public class CurrentThreadUserCulture : IUserCulture
	{
		#region ICultureProvider Members

		public CultureInfo GetCulture()
		{
			return CultureInfo.CurrentCulture;
		}

		#endregion
	}
}