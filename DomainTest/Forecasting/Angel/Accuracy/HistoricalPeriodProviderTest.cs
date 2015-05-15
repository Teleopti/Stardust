using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class HistoricalPeriodProviderTest
	{
		[Test]
		public void ShouldHaveDefaultPeriodIfNoDataForEvaluation()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(null);
			var now = new Now();
			var target = new HistoricalPeriodProvider(now, statisticRepository);

			var result = target.PeriodForEvaluate(workload);
			result.StartDate.Should().Be.EqualTo(now.LocalDateOnly());
			result.EndDate.Should().Be.EqualTo(now.LocalDateOnly());
		}

		[Test]
		public void ShouldHaveDefaultPeriodIfNoDataForForecast()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(null);
			var now = new Now();
			var target = new HistoricalPeriodProvider(now, statisticRepository);

			var result = target.PeriodForForecast(workload);
			result.StartDate.Should().Be.EqualTo(now.LocalDateOnly());
			result.EndDate.Should().Be.EqualTo(now.LocalDateOnly());
		}

		[Test]
		public void ShouldGetMostRecentTwoYearsForEvaluation()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var dateOnly = new DateOnly(2014, 5, 5);
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnly);
			var target = new HistoricalPeriodProvider(new Now(), statisticRepository);

			var result = target.PeriodForEvaluate(workload);
			result.StartDate.Should().Be.EqualTo(new DateOnly(dateOnly.Date.AddYears(-2)));
			result.EndDate.Should().Be.EqualTo(dateOnly);
		}

		[Test]
		public void ShouldGetMostRecentOneYearsForForecast()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var dateOnly = new DateOnly(2014, 5, 5);
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnly);
			var target = new HistoricalPeriodProvider(new Now(), statisticRepository);

			var result = target.PeriodForForecast(workload);
			result.StartDate.Should().Be.EqualTo(new DateOnly(dateOnly.Date.AddYears(-2)));
			result.EndDate.Should().Be.EqualTo(dateOnly);
		}
	}
}