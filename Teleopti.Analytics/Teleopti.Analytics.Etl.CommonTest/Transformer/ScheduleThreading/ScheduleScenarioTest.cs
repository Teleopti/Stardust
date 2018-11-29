using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.ScheduleThreading
{
	[TestFixture]
	public class ScheduleScenarioTest
	{
		[Test]
		public void ShouldHaveCorrectShiftStartAndEndWhenSplitShifts()
		{
			const int minutesPerInterval = 60;
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 2, 8), 
				new Person().WithId(),
				new Scenario("scenario").WithId());
			var shiftStart = new DateTime(2018, 2, 8, 8, 0, 0, DateTimeKind.Utc);
			var shiftEnd = new DateTime(2018, 2, 8, 10, 0, 0, DateTimeKind.Utc);
			var personAssignment = scheduleDay.PersonAssignment(true);
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(shiftStart, new DateTime(2018, 2, 8, 8, 7, 0, DateTimeKind.Utc)));
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 8, 53, 0, DateTimeKind.Utc), shiftEnd));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCat").WithId());
			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			var scheduleProjection = new ScheduleProjection(scheduleDay, scheduleDay.ProjectionService().CreateProjection());
			var jobParamters = JobParametersFactory.SimpleParameters(new JobHelperForScenarioTests(new RaptorRepositoryForTest()), minutesPerInterval);
			var threadObject = new ThreadObj(new List<ScheduleProjection> {scheduleProjection}, DateTime.Now, jobParamters);

			var affectedRows = WorkThreadClass.WorkThread(threadObject);

			affectedRows.Should().Be(3);

			foreach (DataRow row in threadObject.ScheduleTable.Rows)
			{
				var start = (DateTime) row["shift_start"];
				var end = (DateTime) row["shift_end"];
				start.Should().Be(shiftStart);
				end.Should().Be(shiftEnd);
			}
		}

		[Test]
		public void ShouldHaveCorrectShiftStartAndEndWhenSplitShiftsHaveAGapEqualToIntervalLength()
		{
			const int minutesPerInterval = 60;
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 2, 8),
				new Person().WithId(),
				new Scenario("scenario").WithId());
			var shiftStart = new DateTime(2018, 2, 8, 8, 0, 0, DateTimeKind.Utc);
			var shiftEnd = new DateTime(2018, 2, 8, 11, 0, 0, DateTimeKind.Utc);
			var personAssignment = scheduleDay.PersonAssignment(true);
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(shiftStart, new DateTime(2018, 2, 8, 9, 0, 0, DateTimeKind.Utc)));
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 10, 0, 0, DateTimeKind.Utc), shiftEnd));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCat").WithId());
			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			var scheduleProjection = new ScheduleProjection(scheduleDay, scheduleDay.ProjectionService().CreateProjection());
			var jobParamters = JobParametersFactory.SimpleParameters(new JobHelperForScenarioTests(new RaptorRepositoryForTest()), minutesPerInterval);
			var threadObject = new ThreadObj(new List<ScheduleProjection> { scheduleProjection }, DateTime.Now, jobParamters);

			var affectedRows = WorkThreadClass.WorkThread(threadObject);

			affectedRows.Should().Be(2);

			foreach (DataRow row in threadObject.ScheduleTable.Rows)
			{
				var start = (DateTime)row["shift_start"];
				var end = (DateTime)row["shift_end"];
				start.Should().Be(shiftStart);
				end.Should().Be(shiftEnd);
			}
		}

		[Test]
		public void ShouldHaveCorrectShiftStartAndEndWhenNotSplitShiftsChangingActivityMidInterval()
		{
			const int minutesPerInterval = 60;
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 2, 8),
				new Person().WithId(),
				new Scenario("scenario").WithId());
			var shiftStart = new DateTime(2018, 2, 8, 8, 0, 0, DateTimeKind.Utc);
			var shiftEnd = new DateTime(2018, 2, 8, 11, 0, 0, DateTimeKind.Utc);
			var personAssignment = scheduleDay.PersonAssignment(true);
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(shiftStart, new DateTime(2018, 2, 8, 9, 30, 0, DateTimeKind.Utc)));
			personAssignment.AddActivity(new Activity("phone").WithId(),
				new DateTimePeriod(new DateTime(2018, 2, 8, 9, 30, 0, DateTimeKind.Utc), shiftEnd));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCat").WithId());
			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			var scheduleProjection = new ScheduleProjection(scheduleDay, scheduleDay.ProjectionService().CreateProjection());
			var jobParamters = JobParametersFactory.SimpleParameters(new JobHelperForScenarioTests(new RaptorRepositoryForTest()), minutesPerInterval);
			var threadObject = new ThreadObj(new List<ScheduleProjection> { scheduleProjection }, DateTime.Now, jobParamters);

			var affectedRows = WorkThreadClass.WorkThread(threadObject);

			affectedRows.Should().Be(4);

			foreach (DataRow row in threadObject.ScheduleTable.Rows)
			{
				var start = (DateTime)row["shift_start"];
				var end = (DateTime)row["shift_end"];
				start.Should().Be(shiftStart);
				end.Should().Be(shiftEnd);
			}
		}

		[Test]
		public void ShouldHaveCorrectShiftStartAndEndWhenTimesNotMatchingIntervalLength()
		{
			const int minutesPerInterval = 60;
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(2018, 2, 8),
				new Person().WithId(),
				new Scenario("scenario").WithId());
			var shiftStart = new DateTime(2018, 2, 8, 8, 55, 0, DateTimeKind.Utc);
			var shiftEnd = new DateTime(2018, 2, 8, 17, 55, 0, DateTimeKind.Utc);
			var personAssignment = scheduleDay.PersonAssignment(true);
			personAssignment.AddActivity(new Activity("phone").WithId(), new DateTimePeriod(shiftStart, shiftEnd));
			personAssignment.SetShiftCategory(new ShiftCategory("shiftCat").WithId());
			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			var scheduleProjection = new ScheduleProjection(scheduleDay, scheduleDay.ProjectionService().CreateProjection());
			var jobParamters = JobParametersFactory.SimpleParameters(new JobHelperForScenarioTests(new RaptorRepositoryForTest()), minutesPerInterval);
			var threadObject = new ThreadObj(new List<ScheduleProjection> { scheduleProjection }, DateTime.Now, jobParamters);

			var affectedRows = WorkThreadClass.WorkThread(threadObject);

			affectedRows.Should().Be(10);

			foreach (DataRow row in threadObject.ScheduleTable.Rows)
			{
				var start = (DateTime)row["shift_start"];
				var end = (DateTime)row["shift_end"];
				start.Should().Be(shiftStart);
				end.Should().Be(shiftEnd);
			}
		}
	}
}
