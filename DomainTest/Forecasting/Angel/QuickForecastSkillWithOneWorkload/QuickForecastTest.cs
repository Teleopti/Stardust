using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
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
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x =>
					x.LoadDailyStatisticForSpecificDates(Workload.QueueSourceCollection,
						HistoricalPeriodForForecast.ToDateTimePeriod(SkillTimeZoneInfo()), Workload.Skill.TimeZone.Id, Workload.Skill.MidnightBreakOffset))
				.Return(StatisticTasks().ToArray());
			statisticRepository.Stub(
				x =>
					x.LoadDailyStatisticForSpecificDates(Workload.QueueSourceCollection,
						HistoricalPeriodForMeasurement.ToDateTimePeriod(SkillTimeZoneInfo()), Workload.Skill.TimeZone.Id, Workload.Skill.MidnightBreakOffset))
				.Return(StatisticTasksForMeasurement().ToArray());
			repositoryFactory.Stub(x => x.CreateStatisticRepository()).Return(statisticRepository);
			var dailyStatistics = new DailyStatisticsProvider(repositoryFactory);

			var skillDays = CurrentSkillDays();

			var currentScenario = MockRepository.GenerateStub<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(DefaultScenario);

			var futureData =new FutureData();
			var historicalData = new HistoricalData(dailyStatistics);
			var methodProvider = new ForecastMethodProvider(new LinearRegressionTrendCalculator());
			var outlierRemover = new OutlierRemover();
			var quickForecasterWorkload = new QuickForecasterWorkload(historicalData, futureData, methodProvider, new ForecastingTargetMerger(), outlierRemover, MockRepository.GenerateMock<IIntradayForecaster>());
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(Workload)).Return(HistoricalPeriodForForecast);
			var forecastMethodProvider = methodProvider;
			var quickForecastWorkloadEvaluator = new ForecastWorkloadEvaluator(historicalData, new ForecastingWeightedMeanAbsolutePercentageError(), forecastMethodProvider, historicalPeriodProvider, outlierRemover);
			var target = new QuickForecaster(quickForecasterWorkload,
				new FetchAndFillSkillDays(SkillDayRepository(skillDays), currentScenario,
					new SkillDayRepository(MockRepository.GenerateStrictMock<ICurrentUnitOfWork>())), quickForecastWorkloadEvaluator, historicalPeriodProvider);
			target.ForecastWorkloadsWithinSkill(Workload.Skill, new[] { new ForecastWorkloadInput { WorkloadId = Workload.Id.Value, ForecastMethodId = ForecastMethodType.TeleoptiClassicLongTerm } }, FuturePeriod);

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
				var skill = SkillFactory.CreateSkill("_");
				skill.SetId(Guid.NewGuid());
				_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
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