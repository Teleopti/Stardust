using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecasterSkillWithMultipleWorkloadsTest
	{
		[Test]
		public void ShouldCalculateSumOfTasksFromAllWorkloadsOnOneSkill()
		{
			var historicalPeriod = new DateOnlyPeriod(2000,1,1,2000,1,1);
			var futurePeriod = new DateOnlyPeriod(historicalPeriod.StartDate.AddDays(7), historicalPeriod.EndDate.AddDays(7));
			var currentScenario = new FakeCurrentScenario();
			var skill = SkillFactory.CreateSkill("test");
			var wl1 = WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			wl1.AddQueueSource(new QueueSource());
			var wl2 = WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			wl2.AddQueueSource(new QueueSource());
			var dateTimeOnStartPeriod = historicalPeriod.ToDateTimePeriod(skill.TimeZone).StartDateTime.AddHours(12);
			const int tasksOnwl1 = 13;
			const int tasksOnwl2 = 20;

			var validatedVolumeDayRepository = createEmptyValidatedVolumeDayRepository(historicalPeriod);

			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepository.Expect(
				x => x.LoadSpecificDates(wl1.QueueSourceCollection, historicalPeriod.ToDateTimePeriod(skill.TimeZone)))
				.Return(new IStatisticTask[] {new StatisticTask {Interval = dateTimeOnStartPeriod, StatOfferedTasks = tasksOnwl1}});
			statisticRepository.Expect(
				x => x.LoadSpecificDates(wl2.QueueSourceCollection, historicalPeriod.ToDateTimePeriod(skill.TimeZone)))
				.Return(new IStatisticTask[] {new StatisticTask {Interval = dateTimeOnStartPeriod, StatOfferedTasks = tasksOnwl2}});
			var dailyStatistics = new DailyStatisticsAggregator(statisticRepository);

			var skillDays = new List<ISkillDay>();
			var skillDayRepository = MockRepository.GenerateStub<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, skill, currentScenario.Current())).Return(skillDays);

			var quickForecasterWorkload = new QuickForecasterWorkload(new HistoricalData(dailyStatistics, validatedVolumeDayRepository), new FutureData(),new ForecastMethod(), new ForecastingTargetMerger());
			var target = new QuickForecaster(quickForecasterWorkload, new FetchAndFillSkillDays(skillDayRepository, currentScenario, new SkillDayRepository(MockRepository.GenerateStrictMock<ICurrentUnitOfWork>())));
			target.Execute(skill, futurePeriod, historicalPeriod);

			Convert.ToInt32(skillDays.Single().Tasks)
				.Should().Be.EqualTo(tasksOnwl1 + tasksOnwl2);
		}

		private static IValidatedVolumeDayRepository createEmptyValidatedVolumeDayRepository(DateOnlyPeriod historicalPeriod)
		{
			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			validatedVolumeDayRepository.Stub(x => x.FindRange(historicalPeriod, null))
				.IgnoreArguments()
				.Return(Enumerable.Empty<IValidatedVolumeDay>().ToArray());
			return validatedVolumeDayRepository;
		}
	}
}