using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Forecasting.Angel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecastSkillWithOneWorkload
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
			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x => x.LoadSpecificDates(Workload.QueueSourceCollection, HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()))).Return(StatisticTasks().ToArray());
			statisticRepository.Stub(
				x => x.LoadSpecificDates(Workload.QueueSourceCollection, HistoricalPeriodForMeasurement.ToDateTimePeriod(SkillTimeZoneInfo()))).Return(StatisticTasksForMeasurement().ToArray());
			var dailyStatistics = new DailyStatisticsAggregator(statisticRepository);

			var skillDays = CurrentSkillDays();

			var currentScenario = MockRepository.GenerateStub<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(DefaultScenario);

			var futureData =new FutureData();
			var historicalData = new HistoricalData(dailyStatistics);
			var quickForecasterWorkload = new QuickForecasterWorkload(historicalData, futureData, new ForecastMethodProvider(new IndexVolumes(), null), new ForecastingTargetMerger());
			var target = new QuickForecaster(quickForecasterWorkload,
				new FetchAndFillSkillDays(SkillDayRepository(skillDays), currentScenario,
					new SkillDayRepository(MockRepository.GenerateStrictMock<ICurrentUnitOfWork>())), new QuickForecastWorkloadEvaluator(historicalData, new ForecastingWeightedMeanAbsolutePercentageError(), new ForecastMethodProvider(new IndexVolumes(), new LinearRegressionTrend())));
			target.ForecastWorkloadsWithinSkill(Workload.Skill, new[] { new ForecastWorkloadInput { WorkloadId = Workload.Id.Value, ForecastMethodId = ForecastMethodType.TeleoptiClassic } }, FuturePeriod, HistoricalPeriodForForecast, HistoricalPeriodForMeasurement);

			Assert(skillDays);
		}

		protected IScenario DefaultScenario { get; private set; }

		protected virtual DateOnlyPeriod HistoricalPeriodForForecast
		{
			get { return new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1); }
		}

		protected virtual DateOnlyPeriod HistoricalPeriodForMeasurement
		{
			get { return new DateOnlyPeriod(1999, 1, 1, 2000, 1, 1); }
		}

		protected virtual DateOnlyPeriod FuturePeriod
		{
			get { return new DateOnlyPeriod(HistoricalPeriodForForecast.StartDate.AddDays(7), HistoricalPeriodForForecast.EndDate.AddDays(7)); }
		}

		protected virtual IWorkload Workload
		{
			get
			{
				if (_workload != null) return _workload;
				_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(SkillFactory.CreateSkill("_"));
				_workload.SetId(Guid.NewGuid());
				return _workload;
			}
		}

		protected virtual IEnumerable<StatisticTask> StatisticTasks()
		{
			yield break;
		}

		protected virtual IEnumerable<StatisticTask> StatisticTasksForMeasurement()
		{
			var statisticTasks = StatisticTasks().ToList();
			statisticTasks.Add(new StatisticTask { Interval = HistoricalPeriodForMeasurement.StartDate.Date, StatOfferedTasks = 9 });
			return statisticTasks;
		}

		private ICollection<ISkillDay> _currentSkillDays;
		protected virtual ICollection<ISkillDay> CurrentSkillDays()
		{
			return _currentSkillDays ?? (_currentSkillDays = new List<ISkillDay>());
		}

		protected TimeZoneInfo SkillTimeZoneInfo()
		{
			return Workload.Skill.TimeZone;
		}

		protected virtual ISkillDayRepository SkillDayRepository(ICollection<ISkillDay> existingSkillDays)
		{
			var skillDayRepository = MockRepository.GenerateStub<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(FuturePeriod, Workload.Skill, DefaultScenario)).Return(existingSkillDays);
			return skillDayRepository;
		}

		protected abstract void Assert(IEnumerable<ISkillDay> modifiedSkillDays);
	}

	
}