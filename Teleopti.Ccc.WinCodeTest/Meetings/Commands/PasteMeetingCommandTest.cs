using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class PasteMeetingCommandTest
    {
        private MockRepository _mocks;
        private IMeetingClipboardHandler _meetingClipboardHandler;
        private IMeetingOverviewView _meetingOverviewView;
        private PasteMeetingCommand _target;
        private IMeetingChangerAndPersister _changeMeetingAndPersister;
        private ICanModifyMeeting _canModifyMeeting;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _meetingClipboardHandler = _mocks.StrictMock<IMeetingClipboardHandler>();
            _meetingOverviewView = _mocks.StrictMock<IMeetingOverviewView>();
            _changeMeetingAndPersister = _mocks.StrictMock<IMeetingChangerAndPersister>();
            _canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
            _target = new PasteMeetingCommand(_meetingClipboardHandler, _meetingOverviewView, _changeMeetingAndPersister, _canModifyMeeting);

        }

        [Test]
        public void ShouldNotExecuteIfNoDataInClipboard()
        {
			var startTime = new DateTime(2011, 3, 24, 14, 30, 0, DateTimeKind.Utc);
			var endTime = startTime.AddMinutes(30);
			var selectedPeriod = new DateTimePeriod(startTime, endTime);
        	Expect.Call(_meetingOverviewView.SelectedPeriod()).Return(selectedPeriod);
			Expect.Call(_canModifyMeeting.CanExecute).Return(true);
			Expect.Call(_meetingClipboardHandler.HasData()).Return(false);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetNewDateAndTimeFromViewAndSaveMeeting()
        {
            var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            var meeting = _mocks.StrictMock<IMeeting>();
            var startTime = new DateTime(2011, 3, 24, 14, 30, 0, DateTimeKind.Utc);
            var endTime = startTime.AddMinutes(30);
            var selectedPeriod = new DateTimePeriod(startTime, endTime);
            Expect.Call(_canModifyMeeting.CanExecute).Return(true);
            Expect.Call(_meetingClipboardHandler.HasData()).Return(true);
            Expect.Call(_meetingClipboardHandler.GetData()).Return(meeting);
            Expect.Call(_meetingOverviewView.SelectedPeriod()).Return(selectedPeriod).Repeat.Twice();
            Expect.Call(_meetingOverviewView.UserTimeZone).Return(userTimeZone);
            Expect.Call(() => _changeMeetingAndPersister.ChangeStartDateTimeAndPersist(meeting, startTime, TimeSpan.FromMinutes(0), userTimeZone, _meetingOverviewView));
            Expect.Call(_meetingOverviewView.ReloadMeetings);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }

        [Test]
        public void CanExecuteShouldBeFalseIfNotAllowed()
        {
			var startTime = new DateTime(2011, 3, 24, 14, 30, 0, DateTimeKind.Utc);
			var endTime = startTime.AddMinutes(30);
			var selectedPeriod = new DateTimePeriod(startTime, endTime);
			Expect.Call(_meetingOverviewView.SelectedPeriod()).Return(selectedPeriod);
			Expect.Call(_canModifyMeeting.CanExecute).Return(false);
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.False);
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldNotExecuteIfNoSelectedPeriod()
		{
			var startDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
			var endDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            Expect.Call(_meetingOverviewView.SelectedPeriod()).Return(new DateTimePeriod(startDate, endDate));
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }
}