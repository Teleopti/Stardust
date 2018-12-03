using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class CopyMeetingCommandTest
    {
        private MockRepository _mocks;
        private CopyMeetingCommand _target;
        private IMeetingClipboardHandler _meetingClipboardHandler;
        private IMeetingOverviewView _meetingOverviewView;
        private ICanModifyMeeting _canModifyMeeting;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _meetingClipboardHandler = _mocks.StrictMock<IMeetingClipboardHandler>();
            _meetingOverviewView = _mocks.StrictMock<IMeetingOverviewView>();
            _canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
            _target = new CopyMeetingCommand(_meetingClipboardHandler, _meetingOverviewView, _canModifyMeeting);
        }

        [Test]
        public void CanExecuteShouldBeFalseWhenSelectedMeetingIsNull()
        {
            Expect.Call(_meetingOverviewView.SelectedMeeting).Return(null);
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void CanExecuteShouldBeFalseIfMoreThanOneRecurringDate()
        {
            var meeting = _mocks.StrictMock<IMeeting>();
            Expect.Call(_meetingOverviewView.SelectedMeeting).Return(meeting);
           Expect.Call(meeting.GetRecurringDates()).Return(new List<DateOnly> {new DateOnly(), new DateOnly()});
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetSelectedMeetingToClipboard()
        {
            var meeting = _mocks.StrictMock<IMeeting>();
            Expect.Call(_meetingOverviewView.SelectedMeeting).Return(meeting).Repeat.Twice();
            Expect.Call(meeting.GetRecurringDates()).Return(new List<DateOnly> { new DateOnly() });
            Expect.Call(_canModifyMeeting.CanExecute).Return(true);
            Expect.Call(() =>_meetingClipboardHandler.SetData(meeting));
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }

}