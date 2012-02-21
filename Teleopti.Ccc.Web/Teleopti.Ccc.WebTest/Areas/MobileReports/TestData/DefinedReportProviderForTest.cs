namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	using System.Collections.Generic;
	using System.Linq;

	using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	public class DefinedReportProviderForTest : IDefinedReportProvider
	{
		public IEnumerable<DefinedReportInformation> GetDefinedReports()
		{
			return DefinedReports.ReportInformations;
		}

		public IDefinedReport Get(string reportId)
		{
			return this.GetDefinedReports().FirstOrDefault(x => reportId.Equals(x.ReportId));
		}
	}
}