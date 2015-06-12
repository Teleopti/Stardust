using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class HistoricalPeriodProviderTest
	{
		[Test]
		public void ShouldHaveDefaultPeriodIfNoData()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(null);
			var now = new Now();
			var target = new HistoricalPeriodProvider(now, statisticRepository);

			var result = target.AvailablePeriod(workload);
			result.StartDate.Should().Be.EqualTo(now.LocalDateOnly());
			result.EndDate.Should().Be.EqualTo(now.LocalDateOnly());
		}


		[Test]
		public void ShouldGetMostRecentPeriod()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			var dateOnlyPeriod = new DateOnlyPeriod(2012, 5, 5, 2014, 5, 5);
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnlyPeriod);
			var target = new HistoricalPeriodProvider(new Now(), statisticRepository);

			var result = target.AvailablePeriod(workload);
			result.Should().Be.EqualTo(dateOnlyPeriod);
		}

		[Test]
		public void ShouldGetMaximum3Years()
		{
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			var dateOnlyPeriod = new DateOnlyPeriod(2005, 5, 5, 2014, 5, 5);
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnlyPeriod);
			var target = new HistoricalPeriodProvider(new Now(), statisticRepository);

			var result = target.AvailablePeriod(workload);
			result.StartDate.Should().Be.EqualTo(new DateOnly(dateOnlyPeriod.EndDate.Date.AddYears(-3).AddDays(1)));
			result.EndDate.Should().Be.EqualTo(dateOnlyPeriod.EndDate);
		}

		[Test]
		public void EvaluationPartShouldBeOneYearIfAvailablePeriodIsMoreThan2Years()
		{
			var result = HistoricalPeriodProvider.DivideIntoTwoPeriods(new DateOnlyPeriod(2013, 3, 16, 2015, 3, 15));
			result.Should().Be.EqualTo(new DateOnly(2014, 3, 15));
		}

		[Test]
		public void EvaluationPartShouldBeOneThirdIfAvailablePeriodIsLessThan2Years()
		{
			var result = HistoricalPeriodProvider.DivideIntoTwoPeriods(new DateOnlyPeriod(2013, 3, 17, 2015, 3, 15));
			result.Should().Be.EqualTo(new DateOnly(2014, 3, 16));
		}
	}
}