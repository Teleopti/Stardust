using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class CutMeetingCommandTest
    {
        private IMeetingClipboardHandler _meetingClipboardHandler;
        private IMeetingOverviewView _meetingOverviewView;
        private IMeetingRepository _meetingRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private CutMeetingCommand _target;

        private MockRepository _mocks;
        private ICanModifyMeeting _canModifyMeeting;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _meetingClipboardHandler = _mocks.StrictMock<IMeetingClipboardHandler>();
            _meetingOverviewView = _mocks.StrictMock<IMeetingOverviewView>();
            _meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
            _target = new CutMeetingCommand(_meetingClipboardHandler, _meetingOverviewView, _meetingRepository,
                                              _unitOfWorkFactory, _canModifyMeeting);
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
        public void CanExecuteShouldBeFalseWhenRecurring()
        {
            var meeting = _mocks.StrictMock<IMeeting>();
            Expect.Call(_meetingOverviewView.SelectedMeeting).Return(meeting);
            Expect.Call(meeting.GetRecurringDates()).Return(new List<DateOnly> {new DateOnly(), new DateOnly()});
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveMeetingWhenExecuted()
        {
            var meeting = _mocks.StrictMock<IMeeting>();
			var uow = _mocks.DynamicMock<IUnitOfWork>();

			using (_mocks.Record())
			{
				Expect.Call(_meetingOverviewView.SelectedMeeting).Return(meeting).Repeat.Twice();
				Expect.Call(meeting.GetRecurringDates()).Return(new List<DateOnly>());
				Expect.Call(meeting.Snapshot);
				Expect.Call(_canModifyMeeting.CanExecute).Return(true);
				Expect.Call(() => _meetingClipboardHandler.SetData(meeting));
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(() => _meetingRepository.Remove(meeting));
				Expect.Call(uow.PersistAll());
				Expect.Call(_meetingOverviewView.ReloadMeetings);
			}
			using (_mocks.Playback())
			{
				_target.Execute();
				_mocks.VerifyAll();
			}
        }
    }
}