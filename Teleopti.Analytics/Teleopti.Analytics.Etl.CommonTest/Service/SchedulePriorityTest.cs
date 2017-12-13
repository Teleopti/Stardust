using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Service;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	public class SchedulePriorityTest
	{
		[Test]
		public void ShouldReturnEarliestValidDailyJob()
		{
			var target = new SchedulePriority();
			var now = new DateTime(2017, 12, 12, 8, 0, 0);

			var jobSchedules = new List<IEtlJobSchedule>()
			{
				new EtlJobScheduleForTest
				{
					ScheduleId = 1,
					Enabled = false,
					ScheduleType = JobScheduleType.OccursDaily,
					LastTimeStarted = now.AddDays(-1),
					TimeToRunNextJob = now.AddMinutes(-5)
				},
				new EtlJobScheduleForTest
				{
					ScheduleId = 2,
					Enabled = true,
					ScheduleType = JobScheduleType.OccursDaily,
					LastTimeStarted = now.AddDays(-1),
					TimeToRunNextJob = now.AddMinutes(-4)
				},
				new EtlJobScheduleForTest
				{
					ScheduleId = 3,
					Enabled = true,
					ScheduleType = JobScheduleType.OccursDaily,
					LastTimeStarted = now.AddDays(-1),
					TimeToRunNextJob = now.AddMinutes(-3)
				}
			};
			IEtlJobScheduleCollection jobScheduleCollection = new EtlJobScheduleCollectionForTest(jobSchedules);
			var result = target.GetTopPriority(jobScheduleCollection, now, now.AddMinutes(-30));

			result.ScheduleId.Should().Be(2);
		}

		[Test]
		public void ShouldNotReturnDailyJobInFuture()
		{
			var target = new SchedulePriority();
			var now = new DateTime(2017, 12, 12, 8, 0, 0);

			var jobSchedules = new List<IEtlJobSchedule>()
			{
				new EtlJobScheduleForTest
				{
					ScheduleId = 1,
					Enabled = true,
					ScheduleType = JobScheduleType.OccursDaily,
					LastTimeStarted = now.AddDays(-1),
					TimeToRunNextJob = now.AddMinutes(1)
				}
			};
			IEtlJobScheduleCollection jobScheduleCollection = new EtlJobScheduleCollectionForTest(jobSchedules);
			var result = target.GetTopPriority(jobScheduleCollection, now, now.AddMinutes(-30));

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnPeriodicJobWithinValidTime()
		{
			var target = new SchedulePriority();
			var now = new DateTime(2017, 12, 12, 8, 0, 0);

			var jobSchedules = new List<IEtlJobSchedule>()
			{
				new EtlJobScheduleForTest
				{
					ScheduleId = 1,
					Enabled = true,
					ScheduleType = JobScheduleType.Periodic,
					LastTimeStarted = now.AddHours(-1),
					TimeToRunNextJob = now.AddMinutes(-1),
					PeriodicStartingTodayAt = now.AddHours(-2),
					PeriodicEndingTodayAt = now.AddHours(2)
				},
				new EtlJobScheduleForTest
				{
					ScheduleId = 2,
					Enabled = true,
					ScheduleType = JobScheduleType.Periodic,
					LastTimeStarted = now.AddHours(-1),
					TimeToRunNextJob = now.AddMinutes(-2),
					PeriodicStartingTodayAt = now.AddHours(-2),
					PeriodicEndingTodayAt = now.AddHours(2)
				}

			};
			IEtlJobScheduleCollection jobScheduleCollection = new EtlJobScheduleCollectionForTest(jobSchedules);
			var result = target.GetTopPriority(jobScheduleCollection, now, now.AddMinutes(-30));

			result.ScheduleId.Should().Be(2);
		}

		[Test]
		public void ShouldNotReturnPeriodicJobOutsideValidPeriod()
		{
			var target = new SchedulePriority();
			var now = new DateTime(2017, 12, 12, 8, 0, 0);

			var jobSchedules = new List<IEtlJobSchedule>()
			{
				new EtlJobScheduleForTest
				{
					ScheduleId = 1,
					Enabled = true,
					ScheduleType = JobScheduleType.Periodic,
					LastTimeStarted = now.AddHours(-1),
					TimeToRunNextJob = now.AddMinutes(-1),
					PeriodicStartingTodayAt = now.AddHours(-2),
					PeriodicEndingTodayAt = now.AddHours(-1)
				}
			};
			IEtlJobScheduleCollection jobScheduleCollection = new EtlJobScheduleCollectionForTest(jobSchedules);
			var result = target.GetTopPriority(jobScheduleCollection, now, now.AddMinutes(-30));

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldNotReturnJobIfScheduledBeforeServiceStarted()
		{
			var target = new SchedulePriority();
			var now = new DateTime(2017, 12, 12, 8, 0, 0);
			var serviceStarted = now.AddMinutes(-30);

			var jobSchedules = new List<IEtlJobSchedule>()
			{
				new EtlJobScheduleForTest
				{
					ScheduleId = 1,
					Enabled = true,
					ScheduleType = JobScheduleType.OccursDaily,
					LastTimeStarted = now.AddHours(-1),
					TimeToRunNextJob = now.AddMinutes(-31)
				}
			};
			IEtlJobScheduleCollection jobScheduleCollection = new EtlJobScheduleCollectionForTest(jobSchedules);
			var result = target.GetTopPriority(jobScheduleCollection, now, serviceStarted);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldNotReturnJobIfScheduledBeforeLastTimeStarted()
		{
			var target = new SchedulePriority();
			var now = new DateTime(2017, 12, 12, 8, 0, 0);

			var jobSchedules = new List<IEtlJobSchedule>()
			{
				new EtlJobScheduleForTest
				{
					ScheduleId = 1,
					Enabled = true,
					ScheduleType = JobScheduleType.OccursDaily,
					LastTimeStarted = now.AddMinutes(-1),
					TimeToRunNextJob = now.AddMinutes(-2)
				}
			};
			IEtlJobScheduleCollection jobScheduleCollection = new EtlJobScheduleCollectionForTest(jobSchedules);
			var result = target.GetTopPriority(jobScheduleCollection, now, now.AddMinutes(-30));

			result.Should().Be.Null();
		}
	}
}
