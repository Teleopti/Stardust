namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	using System.Collections.Generic;
	using System.Linq;

	using Web.Areas.MobileReports.Core;
	using Web.Areas.MobileReports.Core.Providers;
	using Web.Areas.MobileReports.Models.Domain;

	public class DefinedReportProviderForTest : IDefinedReportProvider
	{
		#region IDefinedReportProvider Members

		public IEnumerable<DefinedReportInformation> GetDefinedReports()
		{
			return DefinedReports.ReportInformations;
		}

		public IDefinedReport Get(string reportId)
		{
			return this.GetDefinedReports().FirstOrDefault(x => reportId.Equals(x.ReportId));
		}

		#endregion
	}
}