using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.FutureData;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class BothStatisticsAndValidatedTest
	{
		[Test]
		public void BothStatAndValidatedTest()
		{
			const int expectedNumberOfTasks = 123;

			var skill = SkillFactory.CreateSkill("_");
			var workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var historicalPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			var futurePeriod = new DateOnlyPeriod(historicalPeriod.StartDate.AddDays(7), historicalPeriod.EndDate.AddDays(7));

			var historicalDailyStatistic = new DailyStatistic(historicalPeriod.StartDate, expectedNumberOfTasks + 1);

			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			var historialWorkloadDay = new WorkloadDay();
			historialWorkloadDay.Create(historicalPeriod.StartDate, workload, new TimePeriod[] { });
			validatedVolumeDayRepository.Stub(x => x.FindRange(historicalPeriod, workload)).Return(new[]
			{
				new ValidatedVolumeDay(workload, historicalPeriod.StartDate)
				{
					TaskOwner = historialWorkloadDay,
					ValidatedTasks = expectedNumberOfTasks
				}
			});


			var dailyStatistics = MockRepository.GenerateStub<IDailyStatisticsAggregator>();
			dailyStatistics.Stub(x => x.LoadDailyStatistics(workload, historicalPeriod)).Return(new[] { historicalDailyStatistic });

			var futureWorkloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, futurePeriod.StartDate);

			var futureSkillDay = new SkillDay(
				futurePeriod.StartDate,
				skill,
				new Scenario("sdfdsf"),
				new[] { futureWorkloadDay },
				Enumerable.Empty<ISkillDataPeriod>());

			var loadSkillDays = MockRepository.GenerateMock<ILoadSkillDaysInDefaultScenario>();
			loadSkillDays.Stub(x => x.FindRange(futurePeriod, skill)).Return(new[] { futureSkillDay });

			var target = new QuickForecaster(new HistoricalDataProvider(dailyStatistics, validatedVolumeDayRepository), loadSkillDays);
			target.Execute(workload, historicalPeriod, futurePeriod);

			futureSkillDay.Tasks.Should().Be.EqualTo(expectedNumberOfTasks);
		}
	}
}