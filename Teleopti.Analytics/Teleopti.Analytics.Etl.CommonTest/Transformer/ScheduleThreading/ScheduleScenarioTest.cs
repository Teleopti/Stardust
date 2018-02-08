using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.ScheduleThreading
{
	[TestFixture]
	public class ScheduleScenarioTest
	{
		[Test]
		public void ShouldHaveCorrectShiftStartAndEndWhenSplitShifts()
		{
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 2, 8), 
				new Person().WithId(),
				new Scenario("scenario").WithId());
			var personAssignment = scheduleDay.PersonAssignment(true);
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 2, 8, 8, 7, 0, DateTimeKind.Utc)));
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 8, 53, 0, DateTimeKind.Utc),
					new DateTime(2018, 2, 8, 10, 0, 0, DateTimeKind.Utc)));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCat").WithId());
			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			var scheduleProjection = new ScheduleProjection(scheduleDay, scheduleDay.ProjectionService().CreateProjection());
			const int minutesPerInterval = 60;
			var jobParamters = JobParametersFactory.SimpleParameters(new JobHelperForScenarioTests(new RaptorRepositoryForTest()), minutesPerInterval);
			var threadObject = new ThreadObj(new List<ScheduleProjection> {scheduleProjection}, DateTime.Now, jobParamters);

			var affectedRows = WorkThreadClass.WorkThread(threadObject);

			affectedRows.Should().Be(3);

			DateTime? shiftStart = null;
			DateTime? shiftEnd = null;
			foreach (DataRow row in threadObject.ScheduleTable.Rows)
			{
				var start = (DateTime) row["shift_start"];
				var end = (DateTime) row["shift_end"];
				if (!shiftStart.HasValue)
					shiftStart = start;
				if (!shiftEnd.HasValue)
					shiftEnd = end;
				start.Should().Be(shiftStart);
				end.Should().Be(shiftEnd);
			}
		}

		[Test]
		public void ShouldHaveCorrectShiftStartAndEndWhenSplitShiftsHaveAGapEqualToIntervalLength()
		{
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 2, 8),
				new Person().WithId(),
				new Scenario("scenario").WithId());
			var personAssignment = scheduleDay.PersonAssignment(true);
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 2, 8, 9, 0, 0, DateTimeKind.Utc)));
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 2, 8, 11, 0, 0, DateTimeKind.Utc)));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCat").WithId());
			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			var scheduleProjection = new ScheduleProjection(scheduleDay, scheduleDay.ProjectionService().CreateProjection());
			const int minutesPerInterval = 60;
			var jobParamters = JobParametersFactory.SimpleParameters(new JobHelperForScenarioTests(new RaptorRepositoryForTest()), minutesPerInterval);
			var threadObject = new ThreadObj(new List<ScheduleProjection> { scheduleProjection }, DateTime.Now, jobParamters);

			var affectedRows = WorkThreadClass.WorkThread(threadObject);

			affectedRows.Should().Be(2);

			DateTime? shiftStart = null;
			DateTime? shiftEnd = null;
			foreach (DataRow row in threadObject.ScheduleTable.Rows)
			{
				var start = (DateTime)row["shift_start"];
				var end = (DateTime)row["shift_end"];
				if (!shiftStart.HasValue)
					shiftStart = start;
				if (!shiftEnd.HasValue)
					shiftEnd = end;
				start.Should().Be(shiftStart);
				end.Should().Be(shiftEnd);
			}
		}

		[Test]
		public void ShouldHaveCorrectShiftStartAndEndWhenNotSplitShiftsChangingActivityMidInterval()
		{
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 2, 8),
				new Person().WithId(),
				new Scenario("scenario").WithId());
			var personAssignment = scheduleDay.PersonAssignment(true);
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2018, 2, 8, 9, 30, 0, DateTimeKind.Utc)));
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 9, 30, 0, DateTimeKind.Utc),
					new DateTime(2018, 2, 8, 11, 0, 0, DateTimeKind.Utc)));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCat").WithId());
			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			var scheduleProjection = new ScheduleProjection(scheduleDay, scheduleDay.ProjectionService().CreateProjection());
			const int minutesPerInterval = 60;
			var jobParamters = JobParametersFactory.SimpleParameters(new JobHelperForScenarioTests(new RaptorRepositoryForTest()), minutesPerInterval);
			var threadObject = new ThreadObj(new List<ScheduleProjection> { scheduleProjection }, DateTime.Now, jobParamters);

			var affectedRows = WorkThreadClass.WorkThread(threadObject);

			affectedRows.Should().Be(4);

			DateTime? shiftStart = null;
			DateTime? shiftEnd = null;
			foreach (DataRow row in threadObject.ScheduleTable.Rows)
			{
				var start = (DateTime)row["shift_start"];
				var end = (DateTime)row["shift_end"];
				if (!shiftStart.HasValue)
					shiftStart = start;
				if (!shiftEnd.HasValue)
					shiftEnd = end;
				start.Should().Be(shiftStart);
				end.Should().Be(shiftEnd);
			}
		}
	}
}
