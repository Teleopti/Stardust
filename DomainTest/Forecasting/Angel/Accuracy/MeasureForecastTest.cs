using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using LinearTrend = Teleopti.Ccc.Domain.Forecasting.Angel.Trend.LinearTrend;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public abstract class MeasureForecastTest
	{
		private IWorkload _workload;

		protected MeasureForecastTest()
		{
			DefaultScenario = new Scenario("default scenario") { DefaultScenario = true };
		}

		[Test]
		public void DoTheTest()
		{
			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x =>
					x.LoadDailyStatisticForSpecificDates(Workload.QueueSourceCollection,
						HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()), Workload.Skill.TimeZone.Id, Workload.Skill.MidnightBreakOffset))
				.Return(StatisticTasks().ToArray());
			var dailyStatistics = new DailyStatisticsProvider(statisticRepository);

			var currentScenario = MockRepository.GenerateStub<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(DefaultScenario);

			var linearTrendMethod = MockRepository.GenerateMock<ILinearTrendCalculator>();
			linearTrendMethod.Stub(x => x.CalculateTrend(null)).IgnoreArguments().Return(new LinearTrend {Slope = 1, Intercept = 2});
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(Workload)).Return(HistoricalPeriodForForecast);

			var outlierRemover = new OutlierRemover();
			var target = new ForecastWorkloadEvaluator(new HistoricalData(dailyStatistics), new ForecastingWeightedMeanAbsolutePercentageError(), new ForecastMethodProvider(new DayWeekMonthIndexVolumes(), linearTrendMethod), historicalPeriodProvider, outlierRemover);
			var measurementResult = target.Evaluate(Workload);

			Assert(measurementResult);
		}

		protected IScenario DefaultScenario { get; private set; }

		protected virtual DateOnlyPeriod HistoricalPeriodForForecast
		{
			get
			{
				var date = new DateTime(2000, 1, 1);
				return new DateOnlyPeriod(new DateOnly(date.AddYears(-1)), new DateOnly(date));
			}
		}

		protected virtual DateOnlyPeriod FuturePeriod
		{
			get { return new DateOnlyPeriod(HistoricalPeriodForForecast.StartDate.AddDays(7), HistoricalPeriodForForecast.EndDate.AddDays(7)); }
		}

		protected virtual IWorkload Workload
		{
			get
			{
				var workload = _workload ?? (_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(SkillFactory.CreateSkillWithId("_")));
				if (!workload.Id.HasValue)
					workload.SetId(Guid.NewGuid());
				return workload;
			}
		}

		protected virtual IEnumerable<StatisticTask> StatisticTasks()
		{
			yield break;
		}

		protected virtual IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			yield break;
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

		protected abstract void Assert(WorkloadAccuracy measurementResult);
	}

	
}