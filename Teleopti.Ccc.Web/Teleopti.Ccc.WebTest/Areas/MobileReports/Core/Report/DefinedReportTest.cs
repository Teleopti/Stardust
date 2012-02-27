using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Report
{
	[TestFixture]
	public class DefinedReportTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
		}

		#endregion

		private MockRepository _mocks;

		[Test]
		public void VerifyUsesDelegateToGenerateReportData()
		{
			IDefinedReport target = new DefinedReportInformation();

			target.GenerateReport = (service, input) => service.GetAnsweredAndAbandoned(input);

			var dataService = _mocks.DynamicMock<IReportDataService>();

			var reportDataParam = new ReportDataParam();
			var reportDataPeriodEntries = new List<ReportDataPeriodEntry>
			                              	{new ReportDataPeriodEntry {Period = "00:00", Y1 = 100M, Y2 = 10M}};

			using (_mocks.Record())
			{
				Expect.Call(dataService.GetAnsweredAndAbandoned(reportDataParam)).Return(reportDataPeriodEntries);
			}

			using (_mocks.Playback())
			{
				IEnumerable<ReportDataPeriodEntry> result = target.GenerateReport(dataService, reportDataParam);

				result.Should().Have.SameSequenceAs(reportDataPeriodEntries);
			}
		}
	}
}