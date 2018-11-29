using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture]
    public class AppointmentFromMeetingCreatorTest
    {
        private MockRepository _mocks;
        private IMeeting _meeting;
        private AppointmentFromMeetingCreator _target;
        readonly DateOnly _startDate = new DateOnly(2011,3,21);
        readonly DateOnly _endDate = new DateOnly(2011,3,27);
        private TimeZoneInfo _timeZone;
        private TimeZoneInfo _userZone;
        private MeetingTimeZoneHelper _meetingTimeZoneHelper;

        [SetUp]
        public void Setup()
        {
            //finland +2
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));
            _userZone = (TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
            _meetingTimeZoneHelper = new MeetingTimeZoneHelper(_userZone);
            _mocks = new MockRepository();
            _meeting = _mocks.StrictMock<IMeeting>();
            _target = new AppointmentFromMeetingCreator(_meetingTimeZoneHelper);

            Expect.Call(_meeting.TimeZone).Return(_timeZone);
            Expect.Call(_meeting.StartTime).Return(TimeSpan.FromHours(12)).Repeat.AtLeastOnce();
            Expect.Call(_meeting.MeetingDuration()).Return(TimeSpan.FromHours(2));
            //Expect.Call(_meeting.EndTime).Return(TimeSpan.FromHours(14)).Repeat.AtLeastOnce();
        }

        [Test]
        public void ShouldCreateAnAppointmentWithTheMeeting()
        {
            Expect.Call(_meeting.GetRecurringDates()).Return(new List<DateOnly> { _startDate.AddDays(1) });
            
            _mocks.ReplayAll();
            var list = _target.GetAppointments(_meeting, _startDate, _endDate);
            Assert.That(list.Count , Is.EqualTo(1));
            Assert.That(list[0].StartDateTime, Is.EqualTo(new DateTime(2011,3,22,10,0,0,DateTimeKind.Utc)));
            Assert.That(list[0].EndDateTime, Is.EqualTo(new DateTime(2011,3,22,12,0,0,DateTimeKind.Utc)));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateMoreAppointmentsWhenRecurrent()
        {
            Expect.Call(_meeting.GetRecurringDates()).Return(new List<DateOnly> { _startDate, _startDate.AddDays(1), _startDate.AddDays(2) });
            Expect.Call(_meeting.MeetingDuration()).Return(TimeSpan.FromHours(2)).Repeat.Twice();//more
            _mocks.ReplayAll();
            var list = _target.GetAppointments(_meeting, _startDate, _endDate);
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 21, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[1].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 22, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[2].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 23, 10, 0, 0, DateTimeKind.Utc)));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldProcessAllInAList()
        {
            Expect.Call(_meeting.GetRecurringDates()).Return(new List<DateOnly> { _startDate, _startDate.AddDays(1), _startDate.AddDays(2), _startDate.AddDays(20) });
            Expect.Call(_meeting.MeetingDuration()).Return(TimeSpan.FromHours(2)).Repeat.Twice(); // more
            _mocks.ReplayAll();
            var list = _target.GetAppointments(new List<IMeeting>{_meeting}, _startDate, _endDate);
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 21, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[1].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 22, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[2].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 23, 10, 0, 0, DateTimeKind.Utc)));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTwoAppointmentsWhenPassingMidnight()
        {
            _mocks.BackToRecordAll();
            Expect.Call(_meeting.TimeZone).Return(_timeZone);
            Expect.Call(_meeting.StartTime).Return(TimeSpan.FromHours(23)).Repeat.AtLeastOnce();
            
            Expect.Call(_meeting.GetRecurringDates()).Return(new List<DateOnly> { _startDate.AddDays(1) });
            Expect.Call(_meeting.MeetingDuration()).Return(TimeSpan.FromHours(4));
            _mocks.ReplayAll();
            var list = _target.GetAppointments(_meeting, _startDate, _endDate);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 22, 21, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[0].EndDateTime, Is.EqualTo(new DateTime(2011, 3, 23, 1, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[1].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 23, 0, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[1].EndDateTime, Is.EqualTo(new DateTime(2011, 3, 23, 1, 0, 0, DateTimeKind.Utc)));
            
            //Assert.That(list[1].EndTime, Is.EqualTo(new TimeSpan( 1, 0, 0)));

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotReturnTwoAppointmentsWhenEndingAtMidnight()
        {
            _mocks.BackToRecordAll();
            Expect.Call(_meeting.TimeZone).Return(_timeZone);
            Expect.Call(_meeting.StartTime).Return(TimeSpan.FromHours(22)).Repeat.AtLeastOnce();
            
            Expect.Call(_meeting.GetRecurringDates()).Return(new List<DateOnly> { _startDate.AddDays(1) });
            Expect.Call(_meeting.MeetingDuration()).Return(TimeSpan.FromHours(4));
            _mocks.ReplayAll();
            var list = _target.GetAppointments(_meeting, _startDate, _endDate);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 22, 20, 0, 0, DateTimeKind.Utc)));
            Assert.That(list[0].EndDateTime, Is.EqualTo(new DateTime(2011, 3, 23, 0, 0, 0, DateTimeKind.Utc)));
            
            _mocks.VerifyAll();
        }

        [Test]
        public void FirstShouldReturnSelfIfNoPrevious()
        {
            var simple = new SimpleAppointment();
            Assert.That(simple.FirstAppointment, Is.EqualTo(simple));
        }

        [Test]
        public void LastShouldReturnSelfIfNoNext()
        {
            var simple = new SimpleAppointment();
            Assert.That(simple.LastAppointment, Is.EqualTo(simple));
        }

        [Test]
        public void ShouldReturnFirstAndLastInChain()
        {
            var simple = new SimpleAppointment();
            var simple2 = new SimpleAppointment();
            var simple3 = new SimpleAppointment();

            simple2.PreviousAppointment = simple;
            simple3.PreviousAppointment = simple2;
            
            Assert.That(simple3.FirstAppointment,Is.EqualTo(simple));
            Assert.That(simple.LastAppointment,Is.EqualTo(simple3));
        }

        [Test]
        public void ShouldCheckIfStartAndEndTimeIsValid()
        {
            var userZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            _target = new AppointmentFromMeetingCreator(new MeetingTimeZoneHelper(userZone));

            _mocks.BackToRecordAll();
            Expect.Call(_meeting.TimeZone).Return(_timeZone);
            Expect.Call(_meeting.StartTime).Return(new TimeSpan(3, 15 ,0)).Repeat.AtLeastOnce();
            Expect.Call(_meeting.MeetingDuration()).Return(TimeSpan.FromMinutes(15));
            
            Expect.Call(_meeting.GetRecurringDates()).Return(new List<DateOnly> { _startDate.AddDays(6) });
            _mocks.ReplayAll();
            var list = _target.GetAppointments(_meeting, _startDate, _endDate);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].StartDateTime, Is.EqualTo(new DateTime(2011, 3, 27, 3, 15, 0, DateTimeKind.Utc)));
            Assert.That(list[0].EndDateTime, Is.EqualTo(new DateTime(2011, 3, 27, 3, 30, 0, DateTimeKind.Utc)));
            _mocks.VerifyAll();
        }
    }
}