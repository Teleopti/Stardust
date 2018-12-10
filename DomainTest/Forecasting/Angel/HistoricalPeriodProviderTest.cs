using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class HistoricalPeriodProviderTest
	{
		[Test]
		public void ShouldReturnNullIfNoData()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(null);
			repositoryFactory.Stub(x => x.CreateStatisticRepository()).Return(statisticRepository);
			var target = new HistoricalPeriodProvider(repositoryFactory);

			var result = target.AvailablePeriod(workload);
			result.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldGetMostRecentPeriod()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			var dateOnlyPeriod = new DateOnlyPeriod(2012, 5, 5, 2014, 5, 5);
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnlyPeriod);
			repositoryFactory.Stub(x => x.CreateStatisticRepository()).Return(statisticRepository);
			var target = new HistoricalPeriodProvider(repositoryFactory);

			var result = target.AvailablePeriod(workload);
			result.Value.Should().Be.EqualTo(dateOnlyPeriod);
		}

		[Test]
		public void ShouldGetMaximum3Years()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var workload = new Workload(SkillFactory.CreateSkill("Phone"));
			var dateOnlyPeriod = new DateOnlyPeriod(2005, 5, 5, 2014, 5, 5);
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(dateOnlyPeriod);
			repositoryFactory.Stub(x => x.CreateStatisticRepository()).Return(statisticRepository);
			var target = new HistoricalPeriodProvider(repositoryFactory);

			var result = target.AvailablePeriod(workload);
			result.Value.StartDate.Should().Be.EqualTo(new DateOnly(dateOnlyPeriod.EndDate.Date.AddYears(-3).AddDays(1)));
			result.Value.EndDate.Should().Be.EqualTo(dateOnlyPeriod.EndDate);
		}

		[Test]
		public void ShouldGetMostRecentPeriodForIntradayTemplatePeriodByPeriod()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2014, 4, 5, 2014, 5, 5);
			var target = new HistoricalPeriodProvider(MockRepository.GenerateMock<IRepositoryFactory>());

			var result = target.AvailableIntradayTemplatePeriod(dateOnlyPeriod);
			result.Should().Be.EqualTo(dateOnlyPeriod);
		}

		[Test]
		public void ShouldGetMaximum3MonthsForIntradayTemplatePeriodByPeriod()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2005, 5, 5, 2014, 5, 5);
			var target = new HistoricalPeriodProvider(MockRepository.GenerateMock<IRepositoryFactory>());

			var result = target.AvailableIntradayTemplatePeriod(dateOnlyPeriod);
			result.StartDate.Should().Be.EqualTo(new DateOnly(dateOnlyPeriod.EndDate.Date.AddMonths(-3).AddDays(1)));
			result.EndDate.Should().Be.EqualTo(dateOnlyPeriod.EndDate);
		}

		[Test]
		public void EvaluationPartShouldBeOneYearIfAvailablePeriodIsMoreThan2Years()
		{
			var result = HistoricalPeriodProvider.DivideIntoTwoPeriods(new DateOnlyPeriod(2013, 3, 16, 2015, 3, 15));
			result.Item1.StartDate.Should().Be.EqualTo(new DateOnly(2013, 3, 16));
			result.Item1.EndDate.Should().Be.EqualTo(new DateOnly(2014, 3, 15));
			result.Item2.StartDate.Should().Be.EqualTo(new DateOnly(2014, 3, 16));
			result.Item2.EndDate.Should().Be.EqualTo(new DateOnly(2015, 3, 15));
		}

		[Test]
		public void EvaluationPartShouldBeOneThirdIfAvailablePeriodIsLessThan2Years()
		{
			var result = HistoricalPeriodProvider.DivideIntoTwoPeriods(new DateOnlyPeriod(2013, 3, 17, 2015, 3, 15));
			result.Item1.StartDate.Should().Be.EqualTo(new DateOnly(2013, 3, 17));
			result.Item1.EndDate.Should().Be.EqualTo(new DateOnly(2014, 3, 16));
			result.Item2.StartDate.Should().Be.EqualTo(new DateOnly(2014, 3, 17));
			result.Item2.EndDate.Should().Be.EqualTo(new DateOnly(2015, 3, 15));
		}

		[Test]
		public void EvaluationPartShouldBeOneThirdIfAvailablePeriodIsLessThan2Years2()
		{
			var result = HistoricalPeriodProvider.DivideIntoTwoPeriods(new DateOnlyPeriod(2013, 3, 1, 2013, 3, 7));
			result.Item1.StartDate.Should().Be.EqualTo(new DateOnly(2013, 3, 1));
			result.Item1.EndDate.Should().Be.EqualTo(new DateOnly(2013, 3, 4));
			result.Item2.StartDate.Should().Be.EqualTo(new DateOnly(2013, 3, 5));
			result.Item2.EndDate.Should().Be.EqualTo(new DateOnly(2013, 3, 7));
		}

		[Test]
		public void EvaluationPartShouldBeOneThirdIfAvailablePeriodIsLessThan2Years3()
		{
			var result = HistoricalPeriodProvider.DivideIntoTwoPeriods(new DateOnlyPeriod(2013, 3, 1, 2013, 3, 1));
			result.Item1.StartDate.Should().Be.EqualTo(new DateOnly(2013, 3, 1));
			result.Item1.EndDate.Should().Be.EqualTo(new DateOnly(2013, 3, 1));
			result.Item2.StartDate.Should().Be.EqualTo(new DateOnly(2013, 3, 1));
			result.Item2.EndDate.Should().Be.EqualTo(new DateOnly(2013, 3, 1));
		}
	}
}