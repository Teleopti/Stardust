using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Report
{
	// ugh just make the calls
	[TestFixture]
	public class TestDataWebReportRepositoryTest
	{
		private readonly TestDataWebReportRepository _target;


		public TestDataWebReportRepositoryTest()
		{
			_target = new TestDataWebReportRepository();
		}

		[Test]
		public void VerifyReportControlSkillGet()
		{
			var result = _target.ReportControlSkillGet(0, Guid.Empty, 0, Guid.Empty);

			result.Count().Should().Be.EqualTo(6);
			result.Any(r => r.Id == -1).Should().Be.True();
			result.Any(r => r.Id == -2).Should().Be.True();
		}

		[Test]
		public void VerifyReportDataForecastVersusActualWorkload()
		{
			var result = _target.ReportDataForecastVersusActualWorkload(0,
			                                                            string.Empty, string.Empty, 1, DateTime.Today,
			                                                            DateTime.Today, 0, 4, 0, Guid.Empty, 0, 0, Guid.Empty);
			result.Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void VerifyReportDataQueueStatAbandoned()
		{
			var result = _target.ReportDataQueueStatAbandoned(0, string.Empty, string.Empty, 1, DateTime.Today, DateTime.Today, 0,
			                                                  4, 0, Guid.Empty, 0, 0, Guid.Empty);
			result.Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void VerifyReportDataServiceLevelAgentsReady()
		{
			var result = _target.ReportDataServiceLevelAgentsReady(string.Empty, string.Empty, 1, DateTime.Today, DateTime.Today,
			                                                       0, 4, 0, 0, Guid.Empty, 0, 0, Guid.Empty);
			result.Count().Should().Be.EqualTo(5);
		}

		[Test]
		public void VerifyReportMobileReportInit()
		{
			var webReportMobileReportInit = _target.ReportMobileReportInit(Guid.Empty, 0, Guid.Empty, string.Empty, string.Empty);
			webReportMobileReportInit.Should().Not.Be.Null();
		}
	}
}