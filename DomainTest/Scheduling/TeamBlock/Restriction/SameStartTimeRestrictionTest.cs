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
    public class SameStartTimeRestrictionTest
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
        private DateTimePeriod _period;
        private TimeZoneInfo _timeZoneInfo;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = new SchedulingOptions();
				_schedulingOptions.UseBlock = true;
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
            _target = new SameStartTimeRestriction(_timeZoneInfo);
            _dateOnly = new DateOnly(2012, 12, 7);
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
             _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            
            _period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
        }

        [Test]
        public void ShouldExtractSameStartTimeRestrictionFromScheduleDay()
        {
            _schedulingOptions.BlockSameStartTime = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 8, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var projectionService1 = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
            var projectionService2 = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro2);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.ProjectionService()).Return(projectionService1);
                Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
                Expect.Call(visualLayerCollection1.Period()).Return(_period);
                Expect.Call(_scheduleDay2.ProjectionService()).Return(projectionService2);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
                Expect.Call(visualLayerCollection2.Period()).Return(period2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
                var expected = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
                                                        new EndTimeLimitation(),
                                                        new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
                var result = _target.ExtractRestriction(dateList,matrixList );
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldExtractEmptyRestrictionWhenHasTwoDifferentStartTimeSchedules()
        {
            _schedulingOptions.BlockSameStartTime = true;
            var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };
            var period2 = new DateTimePeriod(new DateTime(2012, 12, 8, 7, 0, 0, DateTimeKind.Utc),
                                            new DateTime(2012, 12, 8, 8, 30, 0, DateTimeKind.Utc));
            var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
            var projectionService1 = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
            var projectionService2 = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
			using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro1);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly.AddDays(1))).Return(_scheduleDayPro2);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.ProjectionService()).Return(projectionService1);
                Expect.Call(projectionService1.CreateProjection()).Return(visualLayerCollection1);
                Expect.Call(visualLayerCollection1.Period()).Return(_period);
                Expect.Call(_scheduleDay2.ProjectionService()).Return(projectionService2);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(projectionService2.CreateProjection()).Return(visualLayerCollection2);
                Expect.Call(visualLayerCollection2.Period()).Return(period2);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            
            using (_mocks.Playback())
            {
                var result = _target.ExtractRestriction(dateList, matrixList);
                Assert.That(result, Is.Null);
            }
        }

       
    }
}
