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
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
				x => x.LoadSpecificDates(Workload.QueueSourceCollection, HistoricalPeriod.ToDateTimePeriod(SkillTimeZoneInfo()))).Return(StatisticTasks().ToArray());
			var dailyStatistics = new DailyStatisticsAggregator(statisticRepository);

			var currentScenario = MockRepository.GenerateStub<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(DefaultScenario);

			var quickForecasterWorkloadEvaluator = new QuickForecastWorkloadEvaluator(new HistoricalData(dailyStatistics), new ForecastingWeightedMeanAbsolutePercentageError(), new ForecastMethodProvider(new IndexVolumes(), MockRepository.GenerateMock<ILinearRegressionTrend>()));
			var target = new QuickForecastSkillEvaluator(quickForecasterWorkloadEvaluator);
			var measurementResult = target.Measure(Workload.Skill, HistoricalPeriod);

			Assert(measurementResult);
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

		protected abstract void Assert(SkillAccuracy measurementResult);
	}

	
}