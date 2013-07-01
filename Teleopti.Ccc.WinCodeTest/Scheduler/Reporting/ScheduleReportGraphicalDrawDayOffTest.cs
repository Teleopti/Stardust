using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Pdf;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
    [TestFixture]
    public class ScheduleReportGraphicalDrawDayOffTest
    {
        private ScheduleReportGraphicalDrawDayOff _target;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay;
        private DateTimePeriod _timelinePeriod;
        private IPersonDayOff _personDayOff;
        private ReadOnlyCollection<IPersonDayOff> _personDayOffs;
        private Rectangle _projectionRectangle;
        private DateTimePeriod _scheduleDayPeriod;
        private DayOff _dayOff;
        private ReadOnlyCollection<IPersonAssignment> _personAssignments;
        private IPersonAssignment _personAssignment;
        private IEnumerable<IOvertimeShiftLayer> _overtimeShifts;

        [SetUp]
        public void Setup()
        {
            var doc = new PdfDocument();
            var page = doc.Pages.Add();
            _mock = new MockRepository();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _timelinePeriod = new DateTimePeriod(2011, 1, 10, 2011, 1, 12);
            _projectionRectangle = new Rectangle(0, 0, 100, 10);
            _target = new ScheduleReportGraphicalDrawDayOff(_scheduleDay, _timelinePeriod, _projectionRectangle, false, page.Graphics);
            _personDayOff = _mock.StrictMock<IPersonDayOff>();
            _personDayOffs = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff>{_personDayOff});
            var startDateTime = new DateTime(2011, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var endDateTime = startDateTime.AddHours(24);
            _scheduleDayPeriod = new DateTimePeriod(startDateTime, endDateTime);
            _dayOff = new DayOff(startDateTime, TimeSpan.FromHours(36), TimeSpan.FromHours(0), new Description("day off", "do"), Color.Gray, "payrollCode");
            _personAssignment = _mock.StrictMock<IPersonAssignment>();
            _personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>{_personAssignment});
	        _overtimeShifts = new[]
		        {
			        new OvertimeShiftLayer(new Activity("d"), new DateTimePeriod(), MockRepository.GenerateMock<IMultiplicatorDefinitionSet>())
		        };
        }

        [Test]
        public void ShouldReturnScheduleDayPeriodWhenTimelineOutsideScheduleDayPeriod()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                var period = _target.Period();
                Assert.AreEqual(_scheduleDayPeriod, period);
            }
        }

        [Test]
        public void ShouldReturnTimelinePeriodWhenInsideScheduleDayPeriod()
        {
            var startDateTime = new DateTime(2011, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            var endDateTime = startDateTime.AddDays(4);
            var scheduleDayPeriod = new DateTimePeriod(startDateTime, endDateTime);

            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.Period).Return(scheduleDayPeriod).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                var period = _target.Period();
                Assert.AreEqual(_timelinePeriod, period);
            }   
        }

        [Test]
        public void ShouldReturnPersonDayOffWhenSignificantPartIsDayOffAndNoOvertime()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.DayOff);
                Expect.Call(_scheduleDay.PersonDayOffCollection()).Return(_personDayOffs).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.PersonAssignmentCollection()).Return(_personAssignments);
                Expect.Call(_personAssignment.OvertimeLayers).Return(Enumerable.Empty<IOvertimeShiftLayer>());
            }

            using(_mock.Playback())
            {
                var personDayOff = _target.PersonDayOff();
                Assert.AreEqual(_personDayOff, personDayOff);
            }
        }

        [Test]
        public void ShouldReturnNullIfSignificantPartIsDayOffAndPartContainsOvertime()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.DayOff);
                Expect.Call(_scheduleDay.PersonDayOffCollection()).Return(_personDayOffs).Repeat.AtLeastOnce();	
                Expect.Call(_scheduleDay.PersonAssignmentCollection()).Return(_personAssignments);
                Expect.Call(_personAssignment.OvertimeLayers).Return(_overtimeShifts);
            }

            using(_mock.Playback())
            {
                var personDayOff = _target.PersonDayOff();
                Assert.IsNull(personDayOff);
            }
        }

        [Test]
        public void ShouldReturnNullWhenSignificantPartIsNotDayOff()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
            }

            using (_mock.Playback())
            {
                var personDayOff = _target.PersonDayOff();
                Assert.IsNull(personDayOff);
            }       
        }

        [Test]
        public void ShouldReturnTilingBruch()
        {
            var tilingBrush = _target.TilingBrush();
            Assert.IsNotNull(tilingBrush);
            Assert.AreEqual(tilingBrush.Rectangle.Width, _projectionRectangle.Height);
            Assert.AreEqual(tilingBrush.Rectangle.Height, _projectionRectangle.Height);
        }

        [Test]
        public void ShouldNotDrawAndReturnEmptyRectangleWhenNoDayOff()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
            }

            using(_mock.Playback())
            {
                var rectangle = _target.Draw();
                Assert.AreEqual(Rectangle.Empty, rectangle);
            }
        }

        [Test]
        public void ShouldDrawAndReturnRectangleWhenDayOff()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.DayOff);
                Expect.Call(_scheduleDay.Period).Return(_scheduleDayPeriod).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.PersonDayOffCollection()).Return(_personDayOffs).Repeat.AtLeastOnce();
                Expect.Call(_personDayOff.DayOff).Return(_dayOff).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.PersonAssignmentCollection()).Return(_personAssignments);
                Expect.Call(_personAssignment.OvertimeLayers).Return(Enumerable.Empty<IOvertimeShiftLayer>());
            }

            using (_mock.Playback())
            {
                var rectangle = _target.Draw();
                Assert.IsFalse(rectangle.IsEmpty);
            }   
        }
    }
}
