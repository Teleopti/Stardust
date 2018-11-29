using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
    [TestFixture]
    public class SameShiftRestrictionTest
    {
        private MockRepository _mocks;
        private IScheduleRestrictionStrategy _target;
        private SchedulingOptions _schedulingOptions;
        private IScheduleDayEquator _mainShiftEquator;
        private DateOnly _dateOnly;
        private IActivity _activity;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDay _scheduleDay1;
        private IScheduleDayPro _scheduleDayPro;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = new SchedulingOptions();
				_schedulingOptions.UseBlock = true;
            _mainShiftEquator = _mocks.StrictMock<IScheduleDayEquator>();
            _target = new SameShiftRestriction( _mainShiftEquator);
            _dateOnly = new DateOnly(2012, 12, 7);
            _activity = new Activity("bo");
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExtractSameShiftRestrictionFromScheduleDay()
        {
			  _schedulingOptions.BlockSameShift = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var mainShift = EditableShiftFactory.CreateEditorShift(_activity, _period, new ShiftCategory("cat"));
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro);
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.GetEditorShift()).Return(mainShift);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
                {
                    CommonMainShift = mainShift
                };

                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExtractSameShiftRestrictionFromOnePersonOneBlock()
        {
			  _schedulingOptions.BlockSameShift = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var mainShift = EditableShiftFactory.CreateEditorShift(_activity, _period, new ShiftCategory("cat"));
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.GetEditorShift()).Return(mainShift);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
                {
                    CommonMainShift = mainShift
                };

                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExtractSameShiftRestrictionFromOneBlockWhenBothBlockAndTeamScheduling()
        {
			  _schedulingOptions.BlockSameShift = true;
			  _schedulingOptions.UseBlock = true;
			  _schedulingOptions.UseTeam = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var mainShift = EditableShiftFactory.CreateEditorShift(_activity, _period, new ShiftCategory("cat"));
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.GetEditorShift()).Return(mainShift);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
                {
                    CommonMainShift = mainShift
                };

                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }
        
        [Test]
        public void ShouldExtractNullRestrictionWhenHasTwoDifferentSchedules()
        {
			  _schedulingOptions.BlockSameShift = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            var mainShift1 = EditableShiftFactory.CreateEditorShift(_activity, _period, new ShiftCategory("cat"));
            var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 7, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
            var mainShift2 = EditableShiftFactory.CreateEditorShift(_activity, period2, new ShiftCategory("cat"));

            var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(scheduleDayPro2);
                Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.GetEditorShift()).Return(mainShift1);
                Expect.Call(scheduleDay2.GetEditorShift()).Return(mainShift2);
				Expect.Call(scheduleDay2.TimeZone).Return(TimeZoneInfo.Utc);
                Expect.Call(_mainShiftEquator.MainShiftBasicEquals(mainShift2, mainShift1, TimeZoneInfo.Utc)).Return(false);
            }
            using (_mocks.Playback())
            {
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.Null);
            }
        }

       
        [Test]
        public void ShouldExtractSameShiftRestrictionWhenPersonAssignmentIsNull()
        {
			  _schedulingOptions.BlockSameShift = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.GetEditorShift()).Return(null);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldExtractSameShiftRestrictionWhenMainShiftIsNull()
        {
			  _schedulingOptions.BlockSameShift = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay1.GetEditorShift()).Return(null);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldExtractSameShiftRestrictionWhenScheduleIsNull()
        {
			  _schedulingOptions.BlockSameShift = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(null);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

    }
}
