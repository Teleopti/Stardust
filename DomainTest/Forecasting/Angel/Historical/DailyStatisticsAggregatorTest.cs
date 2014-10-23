using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Historical
{
	public class DailyStatisticsAggregatorTest
	{
		[Test]
		public void ShouldAggregateStatisticTasksForTwoDays()
		{
			var workload = new Workload(SkillFactory.CreateSkill("Direct sales"));
			var dateRange = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 2);

			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x => x.LoadSpecificDates(workload.QueueSourceCollection, new DateTimePeriod(2001, 1, 1, 2001, 1, 3)))
				.Return(new List<IStatisticTask>
				{
					new StatisticTask {Interval = new DateTime(2001, 1, 1, 11, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 6},
					new StatisticTask {Interval = new DateTime(2001, 1, 2, 11, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 7},
				});
			var target = new DailyStatisticsAggregator(statisticRepository);
			var result = target.LoadDailyStatistics(workload, dateRange);

			var firstDayInResult = result.First();
			var lastDayInResult = result.Last();
			
			firstDayInResult.Date.Should().Be.EqualTo(dateRange.StartDate);
			firstDayInResult.CalculatedTasks.Should().Be.EqualTo(6);
			lastDayInResult.Date.Should().Be.EqualTo(dateRange.EndDate);
			lastDayInResult.CalculatedTasks.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldAggregateStatisticTasksForSkillLocalDay()
		{
			var workload = new Workload(SkillFactory.CreateSkill("Direct sales"));
			var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			workload.Skill.TimeZone = localTimeZone;
			var dateRange = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 1);

			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x => x.LoadSpecificDates(workload.QueueSourceCollection, dateRange.ToDateTimePeriod(localTimeZone)))
				.Return(new List<IStatisticTask>
				{
					new StatisticTask {Interval = new DateTime(2000, 12, 31, 23, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 6},
					new StatisticTask {Interval = new DateTime(2001, 1, 1, 11, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 7},
				});
			var target = new DailyStatisticsAggregator(statisticRepository);
			var result = target.LoadDailyStatistics(workload, dateRange);

			var onlyDayInResult = result.Single();
			onlyDayInResult.Date.Should().Be.EqualTo(dateRange.StartDate);
			onlyDayInResult.CalculatedTasks.Should().Be.EqualTo(13);
		}

		[Test]
		public void ShouldAggregateStatisticTasksForSkillLocalDayConsideringSkillMidnightBreak()
		{
			var workload = new Workload(SkillFactory.CreateSkill("Direct sales"));
			var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			workload.Skill.TimeZone = localTimeZone;
			workload.Skill.MidnightBreakOffset = TimeSpan.FromHours(2);
			var dateRange = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 1);

			var statisticRepository = MockRepository.GenerateStub<IStatisticRepository>();
			statisticRepository.Stub(
				x => x.LoadSpecificDates(workload.QueueSourceCollection, dateRange.ToDateTimePeriod(localTimeZone)))
				.Return(new List<IStatisticTask>
				{
					new StatisticTask {Interval = new DateTime(2000, 12, 31, 23, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 6},
					new StatisticTask {Interval = new DateTime(2001, 1, 1, 11, 15, 0, DateTimeKind.Utc), StatOfferedTasks = 7},
				});
			var target = new DailyStatisticsAggregator(statisticRepository);
			var result = target.LoadDailyStatistics(workload, dateRange);

			var firstDayInResult = result.First();
			var lastDayInResult = result.Last();

			firstDayInResult.Date.Should().Be.EqualTo(dateRange.StartDate.AddDays(-1));
			firstDayInResult.CalculatedTasks.Should().Be.EqualTo(6);
			lastDayInResult.Date.Should().Be.EqualTo(dateRange.StartDate);
			lastDayInResult.CalculatedTasks.Should().Be.EqualTo(7);
		}
	}
}