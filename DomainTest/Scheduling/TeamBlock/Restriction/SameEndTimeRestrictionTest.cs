using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
    [TestFixture]
    public class SameEndTimeRestrictionTest
    {
        private MockRepository _mocks;
        private IScheduleRestrictionStrategy _target;
        private SchedulingOptions _schedulingOptions;
        private DateOnly _dateOnly;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDay _scheduleDay1;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDayPro _scheduleDayPro2;
        private TimeZoneInfo _timeZoneInfo;
        private IProjectionService _projectionService1;
        private DateTimePeriod _period1;
        private IVisualLayerCollection _visualLayerCollection1;
        private IProjectionService _projectionService2;
        private IVisualLayerCollection _visualLayerCollection2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = new SchedulingOptions();
				_schedulingOptions.UseBlock = true;
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
            _target = new SameEndTimeRestriction(_timeZoneInfo);
            _dateOnly = new DateOnly(2012, 12, 7);
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _projectionService1 = _mocks.StrictMock<IProjectionService>();
            _visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
            _projectionService2 = _mocks.StrictMock<IProjectionService>();
            _visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
            _period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void ShouldExtractEmptyRestrictionWhenHasTwoDifferentEndTimeSchedules()
        {
            _schedulingOptions.BlockSameEndTime = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            
            var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 7, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro2);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
                Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
                Expect.Call(_visualLayerCollection1.Period()).Return(_period1);
                Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
                Expect.Call(_visualLayerCollection2.Period()).Return(period2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
                var result = _target.ExtractRestriction(dateList, matrixList);
				Assert.That(result, Is.Null);
            }
        }

        [Test]
        public void ShouldExtractSameStartAndEndTimeRestrictionFromScheduleDay()
        {
            _schedulingOptions.BlockSameEndTime = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
            var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro2);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
                Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
                Expect.Call(_visualLayerCollection1.Period()).Return(period1);
                Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
                Expect.Call(_visualLayerCollection2.Period()).Return(period2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(TimeSpan.FromHours(8.5), TimeSpan.FromHours(8.5)),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldExtractSameStartAndEndTimeRestrictionFromOneTeamOneDay()
        {
			  _schedulingOptions.UseTeam = true;
            _schedulingOptions.TeamSameEndTime = true;
            var period1 = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
                Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
                Expect.Call(_visualLayerCollection1.Period()).Return(period1);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(TimeSpan.FromHours(8.5), TimeSpan.FromHours(8.5)),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(new List<DateOnly>{_dateOnly }, matrixList);
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldExtractSameEndTimeWhenScheduleIsNull()
        {
            _schedulingOptions.BlockSameEndTime = true;
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

        [Test]
        public void ShouldExtractSameEndTimeWhenSchedulePartPeriodIsNull()
        {
            _schedulingOptions.BlockSameEndTime = true;
            var dateList = new List<DateOnly> { _dateOnly };
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
                Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
                Expect.Call(_visualLayerCollection1.Period()).Return(null);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
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
