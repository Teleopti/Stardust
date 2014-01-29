using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.MyReport.ViewModelFactory
{
	[TestFixture]
	public class MyReportViewModelFactoryTest
	{
		[Test]
		public void ShouldReturnDailyMetricsViewModel()
		{
			var mapper = MockRepository.GenerateMock<IDailyMetricsMapper>();
			var dailyMetricsForDayQuery = MockRepository.GenerateMock<IDailyMetricsForDayQuery>();
			var target = new MyReportViewModelFactory(dailyMetricsForDayQuery, mapper);
			var dataModel = new DailyMetricsForDayResult();
			var viewModel = new DailyMetricsViewModel();
			var date = DateOnly.Today;

			dailyMetricsForDayQuery.Stub(x => x.Execute(date)).Return(dataModel);
			mapper.Stub(x => x.Map(dataModel)).Return(viewModel);

			var result = target.CreateDailyMetricsViewModel(date);

			result.Should().Be.SameInstanceAs(viewModel);
		}
	}
}