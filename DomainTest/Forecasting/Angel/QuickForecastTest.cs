using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public abstract class QuickForecastTest
	{
		private IWorkload _workload;

		protected QuickForecastTest()
		{
			DefaultScenario = new Scenario("default scenario") {DefaultScenario = true};
		}

		protected IScenario DefaultScenario { get; private set; }

		protected virtual DateOnlyPeriod HistoricalPeriod
		{
			get { return new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1); }
		}

		protected virtual DateOnlyPeriod FuturePeriod
		{
			get { return new DateOnlyPeriod(HistoricalPeriod.StartDate.AddDays(7), HistoricalPeriod.EndDate.AddDays(7)); }
		}

		protected virtual IWorkload Workload
		{
			get 
			{
				return _workload ?? (_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(SkillFactory.CreateSkill("_")));
			}
		}

		protected virtual IEnumerable<DailyStatistic> DailyStatistics()
		{
			return Enumerable.Empty<DailyStatistic>();
		}

		protected virtual IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			return Enumerable.Empty<IValidatedVolumeDay>();
		}

		protected virtual ICollection<ISkillDay> CurrentSkillDays()
		{
			return new List<ISkillDay>();
		}
		protected abstract void Assert(IEnumerable<ISkillDay> modifiedSkillDays);

		[Test]
		public void DoTheTest()
		{
			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			validatedVolumeDayRepository.Stub(x => x.FindRange(HistoricalPeriod, Workload)).Return(ValidatedVolumeDays().ToList());

			var dailyStatistics = MockRepository.GenerateStub<IDailyStatisticsAggregator>();
			dailyStatistics.Stub(x => x.LoadDailyStatistics(Workload, HistoricalPeriod)).Return(DailyStatistics());

			var skillDays = CurrentSkillDays();
			var skillDayRepository = MockRepository.GenerateStub<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(FuturePeriod, Workload.Skill, DefaultScenario)).Return(skillDays);

			var currentScenario = MockRepository.GenerateStub<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(DefaultScenario);

			var futureData =
				new FutureData(new FetchAndFillSkillDays(skillDayRepository, currentScenario,
					new SkillDayRepository(MockRepository.GenerateStrictMock<ICurrentUnitOfWork>())));
			var target = new QuickForecaster(new HistoricalData(dailyStatistics, validatedVolumeDayRepository), futureData, new ForecastVolumeApplier());
			target.Execute(Workload, HistoricalPeriod, FuturePeriod);

			Assert(skillDays);
		}
	}
}