using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
    [TestFixture]
    public class SameShiftRestrictionTest
    {
        private MockRepository _mocks;
        private ISameShiftRestriction _target;
        private ISchedulingOptions _schedulingOptions;
        private IScheduleDayEquator _mainShiftEquator;
        private TimeZoneInfo _timeZoneInfo;
        private INightlyRestRestrictionForTeamBlock _nightlyRestRestrictionForTeamBlock;
        private ITeamBlockInfo _teamBlockInfo;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = new SchedulingOptions();
            _schedulingOptions.UseTeamBlockPerOption = true;
            _mainShiftEquator = _mocks.StrictMock<IScheduleDayEquator>();
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
            _nightlyRestRestrictionForTeamBlock = _mocks.StrictMock<INightlyRestRestrictionForTeamBlock>();
            _target = new SameShiftRestriction( _mainShiftEquator);
            _teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
        }

        [Test]
        public void ShouldNotAddCommonShiftToTheRestrictionIfNotLevelingIsSameShift()
        {
            _schedulingOptions.UseTeamBlockSameShift = false;

            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly };
            var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_nightlyRestRestrictionForTeamBlock.AggregatedNightlyRestRestriction(_teamBlockInfo)).Return(new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>()));
            }


            var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                           new EndTimeLimitation(),
                                                           new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
            var result = _target.ExtractRestriction(dateList, matrixList);
            Assert.That(result, Is.EqualTo(expected));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldExtractSameShiftRestrictionFromScheduleDay()
        {
            _schedulingOptions.UseTeamBlockSameShift = true;
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly };
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            IActivity activity = new Activity("bo");
            var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
            var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, new ShiftCategory("cat"));
            var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay1);
                Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(scheduleDay1.GetEditorShift()).Return(mainShift);
                Expect.Call(_nightlyRestRestrictionForTeamBlock.AggregatedNightlyRestRestriction(_teamBlockInfo)).Return(new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>()));
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
            _schedulingOptions.UseTeamBlockSameShift = true;
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly };
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            IActivity activity = new Activity("bo");
            var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
            var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, new ShiftCategory("cat"));
            var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay1);
                Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(scheduleDay1.GetEditorShift()).Return(mainShift);
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
            _schedulingOptions.UseTeamBlockSameShift = true;
            _schedulingOptions.UseTeamBlockPerOption = true;
            _schedulingOptions.UseGroupScheduling = true;
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly };
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            IActivity activity = new Activity("bo");
            var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
            var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, new ShiftCategory("cat"));
            var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };
            using (_mocks.Record())
            {
                Expect.Call(scheduleMatrixPro1.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay1);
                Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(scheduleDay1.GetEditorShift()).Return(mainShift);
                Expect.Call(_nightlyRestRestrictionForTeamBlock.AggregatedNightlyRestRestriction(_teamBlockInfo)).Return(new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>()));
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
            _schedulingOptions.UseTeamBlockSameShift = true;
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            IActivity activity = new Activity("bo");
            var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
            var mainShift1 = EditableShiftFactory.CreateEditorShift(activity, period1, new ShiftCategory("cat"));
            var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 7, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
            var mainShift2 = EditableShiftFactory.CreateEditorShift(activity, period2, new ShiftCategory("cat"));

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
                Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(scheduleDay1.GetEditorShift()).Return(mainShift1);
                Expect.Call(scheduleDay2.GetEditorShift()).Return(mainShift2);
                Expect.Call(_mainShiftEquator.MainShiftBasicEquals(mainShift2, mainShift1)).Return(false);
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
            _schedulingOptions.UseTeamBlockSameShift = true;
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly };
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay1);
                Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(scheduleDay1.GetEditorShift()).Return(null);
                Expect.Call(_nightlyRestRestrictionForTeamBlock.AggregatedNightlyRestRestriction(_teamBlockInfo)).Return(new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>()));
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
            _schedulingOptions.UseTeamBlockSameShift = true;
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly };
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
                Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay1);
                Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(scheduleDay1.GetEditorShift()).Return(null);
                Expect.Call(_nightlyRestRestrictionForTeamBlock.AggregatedNightlyRestRestriction(_teamBlockInfo)).Return(new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>()));
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
            _schedulingOptions.UseTeamBlockSameShift = true;
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly };
            var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(null);
                Expect.Call(_nightlyRestRestrictionForTeamBlock.AggregatedNightlyRestRestriction(_teamBlockInfo)).Return(new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>()));
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
