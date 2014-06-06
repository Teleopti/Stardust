using System.Collections.Generic;
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
			var target = new MyReportViewModelFactory(dailyMetricsForDayQuery, null, mapper, null, null, null);
			var dataModel = new DailyMetricsForDayResult();
			var viewModel = new DailyMetricsViewModel();
			var date = DateOnly.Today;

			dailyMetricsForDayQuery.Stub(x => x.Execute(date)).Return(dataModel);
			mapper.Stub(x => x.Map(dataModel)).Return(viewModel);

			var result = target.CreateDailyMetricsViewModel(date);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldReturnDetailedAdherenceViewModel()
		{
			var mapper = MockRepository.GenerateMock<IDetailedAdherenceMapper>();
			var detailedAdherenceForDayQuery = MockRepository.GenerateMock<IDetailedAdherenceForDayQuery>();
			var target = new MyReportViewModelFactory(null,detailedAdherenceForDayQuery,null, mapper, null, null);
			var dataModel = new List<DetailedAdherenceForDayResult>();
			var viewModel = new DetailedAdherenceViewModel();
			var date = DateOnly.Today;

			detailedAdherenceForDayQuery.Stub(x => x.Execute(date)).Return(dataModel);
			mapper.Stub(x => x.Map(dataModel)).Return(viewModel);

			var result = target.CreateDetailedAherenceViewModel(date);

			result.Should().Be.SameInstanceAs(viewModel);
		}
	}

	
}