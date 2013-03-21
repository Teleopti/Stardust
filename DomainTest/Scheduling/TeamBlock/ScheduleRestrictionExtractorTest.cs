using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class ScheduleRestrictionExtractorTest
	{
		private MockRepository _mocks;
		private IScheduleRestrictionExtractor _target;
		private ISchedulingOptions _schedulingOptions;
		private IScheduleDayEquator _mainShiftEquator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new SchedulingOptions();
			_schedulingOptions.UseLevellingSameShift = true;
			_schedulingOptions.UseLevellingPerOption = true;
			_mainShiftEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_target = new ScheduleRestrictionExtractor(_mainShiftEquator);
		}

		[Test]
		public void ShouldNotAddCommonShiftToTheRestrictionIfNotLevelingIsSameShift()
		{
			_schedulingOptions.UseLevellingSameShift = false;

			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly };
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };

			var expected = new EffectiveRestriction(new StartTimeLimitation(),
														   new EndTimeLimitation(),
														   new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var result = _target.Extract(dateList, matrixList, _schedulingOptions);
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void ShouldExtractRestrictionFromScheduleDay()
		{
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var personAssignment = _mocks.StrictMock<IPersonAssignment>();
			IActivity activity = new Activity("bo");
			var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, period, new ShiftCategory("cat"));
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
				Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay1.AssignmentHighZOrder()).Return(personAssignment);
				Expect.Call(personAssignment.MainShift).Return(mainShift);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
				                                        new EndTimeLimitation(),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
					{
						CommonMainShift = mainShift
					};

				var result = _target.Extract(dateList, matrixList, _schedulingOptions);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractEmptyRestrictionWhenHasTwoDifferentSchedules()
		{
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
			var personAssignment2 = _mocks.StrictMock<IPersonAssignment>();
			IActivity activity = new Activity("bo");
			var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			IMainShift mainShift1 = MainShiftFactory.CreateMainShift(activity, period1, new ShiftCategory("cat"));
			var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 7, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
			IMainShift mainShift2 = MainShiftFactory.CreateMainShift(activity, period2, new ShiftCategory("cat"));
			
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayPro2);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay1.AssignmentHighZOrder()).Return(personAssignment1);
				Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(scheduleDay2.AssignmentHighZOrder()).Return(personAssignment2);
				Expect.Call(personAssignment1.MainShift).Return(mainShift1);
				Expect.Call(personAssignment2.MainShift).Return(mainShift2);
                Expect.Call(_mainShiftEquator.MainShiftBasicEquals(mainShift2, mainShift1)).Return(false);
			}
			using (_mocks.Playback())
			{
				var result = _target.Extract(dateList, matrixList, _schedulingOptions);
				Assert.That(result, Is.Null);
			}
		}
	}
}
