using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	[TestFixture]
	public class DefinedReportProviderTest
	{
		[Test]
		public void ShouldReturnDefinedReportById()
		{
			string reportId = "GetForeCastVsActualWorkload";

			IDefinedReportProvider target = new DefinedReportProvider();

			var report = target.Get(reportId);

			report.ReportId.Should().Be.EqualTo(reportId);
		}

		[Test]
		public void ShouldReturnDefinedReports()
		{
			IDefinedReportProvider target = new DefinedReportProvider();

			var definedReportInformations = target.GetDefinedReports();

			definedReportInformations.Count().Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldReturnNullIfDefinedReportNotFound()
		{
			const string reportId = "NonExistent";

			IDefinedReportProvider target = new DefinedReportProvider();

			var report = target.Get(reportId);

			report.Should().Be.Null();
		}

		[Test]
		public void VerifyAllReportsHasDefinedRequiredAttributes()
		{
			IDefinedReportProvider target = new DefinedReportProvider();

			var definedReportInformations = target.GetDefinedReports();

			foreach (var definedReportInformation in definedReportInformations)
			{
				definedReportInformation.GenerateReport.Should().Not.Be.Null();
				definedReportInformation.LegendResourceKeys.Should().Not.Be.Null();
			}
		}
	}
}