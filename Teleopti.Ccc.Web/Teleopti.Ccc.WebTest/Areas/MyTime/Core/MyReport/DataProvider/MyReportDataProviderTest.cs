using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.MyReport.DataProvider
{
	[TestFixture]
	public class MyReportDataProviderTest
	{
		[Test]
		public void ShouldRetrieveDailyMetricsData()
		{
			var repository = MockRepository.GenerateMock<IDailyMetricsForDayQuery>();
			var target = new MyReportDataProvider(repository);
			var date = DateOnly.Today;
			var dataModel = new DailyMetricsForDayResult();

			repository.Stub(x => x.Execute(date)).Return(dataModel);

			var result = target.RetrieveDailyMetricsData(date);

			result.Should().Be.SameInstanceAs(dataModel);
		}
	}
}
