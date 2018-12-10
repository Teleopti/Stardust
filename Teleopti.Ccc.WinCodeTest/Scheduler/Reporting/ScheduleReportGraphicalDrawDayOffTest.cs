using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Pdf;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
    [TestFixture]
    public class ScheduleReportGraphicalDrawDayOffTest
    {
        private ScheduleReportGraphicalDrawDayOff _target;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay;
        private DateTimePeriod _timelinePeriod;
        private Rectangle _projectionRectangle;
        private DateTimePeriod _scheduleDayPeriod;
        private DayOff _dayOff;
        private IPersonAssignment _personAssignment;
        private IEnumerable<OvertimeShiftLayer> _overtimeShifts;

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
            var startDateTime = new DateTime(2011, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var endDateTime = startDateTime.AddHours(24);
            _scheduleDayPeriod = new DateTimePeriod(startDateTime, endDateTime);
            _dayOff = new DayOff(startDateTime, TimeSpan.FromHours(36), TimeSpan.FromHours(0), new Description("day off", "do"), Color.Gray, "payrollCode", Guid.NewGuid());
            _personAssignment = _mock.StrictMock<IPersonAssignment>();
	        _overtimeShifts = new[]
		        {
			        new OvertimeShiftLayer(new Activity("d"),new DateTimePeriod(2011, 1, 10, 8, 2011, 1, 10, 9), MockRepository.GenerateMock<IMultiplicatorDefinitionSet>())
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
                Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.OvertimeActivities()).Return(Enumerable.Empty<OvertimeShiftLayer>());
	            Expect.Call(_personAssignment.DayOff()).Return(_dayOff).Repeat.AtLeastOnce();
            }

            using(_mock.Playback())
            {
                var personDayOff = _target.PersonDayOff();
				Assert.AreEqual(_dayOff, personDayOff);
            }
        }

        [Test]
        public void ShouldReturnNullIfSignificantPartIsDayOffAndPartContainsOvertime()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.DayOff);
                Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.OvertimeActivities()).Return(_overtimeShifts);
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
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.OvertimeActivities()).Return(Enumerable.Empty<OvertimeShiftLayer>());
	            Expect.Call(_personAssignment.DayOff()).Return(_dayOff);
            }

            using (_mock.Playback())
            {
                var rectangle = _target.Draw();
                Assert.IsFalse(rectangle.IsEmpty);
            }   
        }
    }
}
