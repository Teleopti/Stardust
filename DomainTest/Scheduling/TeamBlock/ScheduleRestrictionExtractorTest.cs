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
		private TimeZoneInfo _timeZoneInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new SchedulingOptions();
			_schedulingOptions.UseLevellingPerOption = true;
			_mainShiftEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
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
			var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void ShouldExtractSameShiftRestrictionFromScheduleDay()
		{
			_schedulingOptions.UseLevellingSameShift = true;
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

				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractSameShiftCategoryRestrictionFromScheduleDay()
		{
			_schedulingOptions.UseLevellingSameShiftCategory = true;
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var personAssignment = _mocks.StrictMock<IPersonAssignment>();
			IActivity activity = new Activity("bo");
			var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
			                                new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var shiftCategory = new ShiftCategory("cat");
			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, period, shiftCategory);
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
						ShiftCategory = shiftCategory
					};

				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);

				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractNullRestrictionWhenHasTwoDifferentSchedules()
		{
			_schedulingOptions.UseLevellingSameShift = true;
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
				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldExtractSameStartTimeRestrictionFromScheduleDay()
		{
			_schedulingOptions.UseLevellingSameStartTime = true;
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var projectionService2 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
           
			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayPro2);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1);
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
				Expect.Call(visualLayerCollection1.Period()).Return(period1);
				Expect.Call(scheduleDay2.ProjectionService()).Return(projectionService2);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
				Expect.Call(visualLayerCollection2.Period()).Return(period2);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
				                                        new EndTimeLimitation(),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractNullRestrictionWhenHasTwoDifferentStartTimeSchedules()
		{
			_schedulingOptions.UseLevellingSameStartTime = true;
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 7, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var projectionService2 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
           
			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayPro2);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1);
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
				Expect.Call(visualLayerCollection1.Period()).Return(period1);
				Expect.Call(scheduleDay2.ProjectionService()).Return(projectionService2);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
				Expect.Call(visualLayerCollection2.Period()).Return(period2);
			}
			using (_mocks.Playback())
			{
				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldExtractSameEndTimeRestrictionFromScheduleDay()
		{
			_schedulingOptions.UseLevellingSameEndTime = true;
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var projectionService2 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
           
			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayPro2);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1);
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
				Expect.Call(visualLayerCollection1.Period()).Return(period1);
				Expect.Call(scheduleDay2.ProjectionService()).Return(projectionService2);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
				Expect.Call(visualLayerCollection2.Period()).Return(period2);
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(TimeSpan.FromHours(8.5), TimeSpan.FromHours(8.5)),
				                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
				Assert.That(result, Is.EqualTo(expected));
			}
		}

		[Test]
		public void ShouldExtractNullRestrictionWhenHasTwoDifferentEndTimeSchedules()
		{
			_schedulingOptions.UseLevellingSameEndTime = true;
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 7, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var projectionService2 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
           
			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro1);
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayPro2);
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1);
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
				Expect.Call(visualLayerCollection1.Period()).Return(period1);
				Expect.Call(scheduleDay2.ProjectionService()).Return(projectionService2);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
				Expect.Call(visualLayerCollection2.Period()).Return(period2);
			}
			using (_mocks.Playback())
			{
				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldExtractSameStartAndEndTimeRestrictionFromScheduleDay()
		{
			_schedulingOptions.UseLevellingSameStartTime = true;
			_schedulingOptions.UseLevellingSameEndTime = true;
			var dateOnly = new DateOnly(2012, 12, 7);
			var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
			var projectionService1 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var projectionService2 = _mocks.StrictMock<IProjectionService>();
			var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro1).Repeat.Twice();
				Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly.AddDays(1))).Return(scheduleDayPro2).Repeat.Twice();
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1).Repeat.Twice();
				Expect.Call(scheduleDay1.ProjectionService()).Return(projectionService1).Repeat.Twice();
				Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1).Repeat.Twice();
				Expect.Call(visualLayerCollection1.Period()).Return(period1).Repeat.Twice();
				Expect.Call(scheduleDay2.ProjectionService()).Return(projectionService2).Repeat.Twice();
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2).Repeat.Twice();
				Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2).Repeat.Twice();
				Expect.Call(visualLayerCollection2.Period()).Return(period2).Repeat.Twice();
			}
			using (_mocks.Playback())
			{
				var expected = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
														new EndTimeLimitation(TimeSpan.FromHours(8.5), TimeSpan.FromHours(8.5)),
														new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
				var result = _target.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo);
				Assert.That(result, Is.EqualTo(expected));
			}
		}
	}
}
