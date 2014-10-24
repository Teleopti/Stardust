using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecast
{
	public abstract class QuickForecastTest
	{
		private IWorkload _workload;

		protected QuickForecastTest()
		{
			DefaultScenario = new Scenario("default scenario") {DefaultScenario = true};
		}

		[Test]
		public void DoTheTest()
		{
			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			validatedVolumeDayRepository.Stub(x => x.FindRange(HistoricalPeriod, Workload)).Return(ValidatedVolumeDays().ToList());

			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x => x.LoadSpecificDates(Workload.QueueSourceCollection, HistoricalPeriod.ToDateTimePeriod(SkillTimeZoneInfo()))).Return(StatisticTasks().ToArray());
			var dailyStatistics = new DailyStatisticsAggregator(statisticRepository);

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
			get { return _workload ?? (_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(SkillFactory.CreateSkill("_"))); }
		}

		protected virtual IEnumerable<StatisticTask> StatisticTasks()
		{
			return Enumerable.Empty<StatisticTask>();
		}

		protected virtual IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			return Enumerable.Empty<IValidatedVolumeDay>();
		}

		protected virtual ICollection<ISkillDay> CurrentSkillDays()
		{
			return new List<ISkillDay>();
		}

		protected TimeZoneInfo SkillTimeZoneInfo()
		{
			return Workload.Skill.TimeZone;
		}

		protected abstract void Assert(IEnumerable<ISkillDay> modifiedSkillDays);
	}
}