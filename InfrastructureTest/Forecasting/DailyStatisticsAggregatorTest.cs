using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Forecasting
{
	public class DailyStatisticsAggregatorTest
	{
		[Test]
		public void ShouldAggregateStatisticTasksForOneDay()
		{
			var workload = new Workload(SkillFactory.CreateSkill("Direct sales"));
			var dateRange = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 1);

			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x => x.LoadSpecificDates(workload.QueueSourceCollection, new DateTimePeriod(2001, 1, 1, 2001, 1, 2)))
				.Return(new List<IStatisticTask>
				{
					new StatisticTask {Interval = new DateTime(2001, 1, 1, 11, 15, 0, DateTimeKind.Utc), StatAnsweredTasks = 6},
					new StatisticTask {Interval = new DateTime(2001, 1, 1, 11, 30, 0, DateTimeKind.Utc), StatAnsweredTasks = 7},
				});
			var target = new DailyStatisticsAggregator(statisticRepository);
			var result = target.LoadDailyStatistics(workload, dateRange);

			result.Single().Date.Should().Be.EqualTo(dateRange.StartDate);
			result.Single().CalculatedTasks.Should().Be.EqualTo(13);
		}
	}
}