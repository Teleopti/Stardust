namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core
{
	using NUnit.Framework;

	using SharpTestsEx;

	using Teleopti.Ccc.Web.Areas.MobileReports.Core;

	[TestFixture]
	public class DefinedReportsTest
	{
		[Test]
		public void VerifyAllReportsHasDefinedRequiredAttributes()
		{
			var definedReportInformations = DefinedReports.ReportInformations;

			foreach (var definedReportInformation in definedReportInformations)
			{
				definedReportInformation.GenerateReport.Should().Not.Be.Null();
				definedReportInformation.ReportInfo.Should().Not.Be.Null();
				definedReportInformation.ReportInfo.SeriesResourceKeys.Should().Not.Be.Null();
				definedReportInformation.ReportInfo.ChartTypeHint.Should().Not.Be.Null();
				definedReportInformation.ReportInfo.SeriesFixedDecimalHint.Should().Not.Be.Null();
			}
		}
	}
}