using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Report
{
	[TestFixture]
	public class ReportRequestValidatorResultTest
	{
		[Test]
		public void ShouldReportInValidIfErrorsPresent()
		{
			var target = new ReportDataFetchResult {Errors = new[] {"Error"}};

			target.IsValid().Should().Be.False();
		}

		[Test]
		public void ShouldReportValidWhenErrorIsNull()
		{
			var target = new ReportDataFetchResult();

			target.IsValid().Should().Be.True();
		}
	}
}