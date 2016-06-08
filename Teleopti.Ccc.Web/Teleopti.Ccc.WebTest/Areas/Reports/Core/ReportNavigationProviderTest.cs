using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Reports.Core
{
	[TestFixture]
	public class ReportNavigationProviderTest
	{
		private FakeReportProvider _reportProvider;
		private IReportUrl _reportUrl;

		[SetUp]
		public void SetUp()
		{
			_reportUrl = new FakeReportUrl();
			_reportProvider = new FakeReportProvider();
		}

		[Test]
		public void ShouldGetNavigationItemsWhenHasPermission()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");
			_reportProvider.PermitReport(report);
			var target = new ReportNavigationProvider(_reportProvider, _reportUrl);

			var result = target.GetNavigationItems();

			result.Should().Not.Be.Null();
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetNoReportItemsWhenHasNoPermission()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");

			var target = new ReportNavigationProvider(_reportProvider, _reportUrl);

			var result = target.GetNavigationItems();

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotGetSpecifyReportItemWhenHasNoPermissionOfTheReport()
		{
			var report = MockRepository.GenerateMock<IApplicationFunction>();
			report.Stub(x => x.LocalizedFunctionDescription).Return("report1");
			report.Stub(x => x.ForeignId).Return("report1");

			var report2 = MockRepository.GenerateMock<IApplicationFunction>();
			report2.Stub(x => x.LocalizedFunctionDescription).Return("report2");
			report2.Stub(x => x.ForeignId).Return("report2");

			_reportProvider.PermitReport(report);

			var target = new ReportNavigationProvider(_reportProvider, _reportUrl);

			var result = target.GetNavigationItems();

			result.Count.Should().Be.EqualTo(1);
			result.SingleOrDefault().Url.Should().Be.EqualTo("Selection.aspx?ReportId=report1&BuId=00000001");
			result.SingleOrDefault().Name.Should().Be.EqualTo("report1");
		}
	}


	public class FakeReportProvider : IReportsProvider
	{
		private readonly IList<IApplicationFunction> permittedReports = new List<IApplicationFunction>();

		public void PermitReport(IApplicationFunction report)
		{
			permittedReports.Add(report);
		}
		public IEnumerable<IApplicationFunction> GetReports()
		{
			return permittedReports;
		}
	}

	public class FakeReportUrl : IReportUrl
	{
		private string matrixUrl = "Selection.aspx?ReportId={0}&BuId=00000001";
		public string Build(string reportId)
		{
			return string.Format(matrixUrl, reportId);
		}
	}
}