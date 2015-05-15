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
		public void ShouldGetMostRecentTwoYearsForEvaluation()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			var dateOnlyPeriod = new DateOnlyPeriod(2012, 5, 5, 2014, 5, 5);
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnlyPeriod);
			var target = new HistoricalPeriodProvider(new Now(), statisticRepository);

			var result = target.PeriodForEvaluate(workload);
			result.Should().Be.EqualTo(dateOnlyPeriod);
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
		public void ShouldGetMostRecentTwoYearsForForecast()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			var dateOnlyPeriod = new DateOnlyPeriod(2012, 5, 5, 2014, 5, 5);
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnlyPeriod);
			var target = new HistoricalPeriodProvider(new Now(), statisticRepository);

			var result = target.PeriodForForecast(workload);
			result.Should().Be.EqualTo(dateOnlyPeriod);
		}

		[Test]
		public void ShouldGetMostRecentOneYearForDisplay()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			var dateOnlyPeriod = new DateOnlyPeriod(2013, 5, 5, 2014, 5, 5);
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnlyPeriod);
			var target = new HistoricalPeriodProvider(new Now(), statisticRepository);

			var result = target.PeriodForDisplay(workload);
			result.Should().Be.EqualTo(dateOnlyPeriod);
		}
	}
}