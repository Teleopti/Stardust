using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class DeleteMeetingCommandTest
    {
        private MockRepository _mocks;
        private DeleteMeetingCommand _target;
        private IMeetingOverviewView _view;
        private IMeetingRepository _meetingRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ICanModifyMeeting _canModifyMeeting;
        private IMeetingParticipantPermittedChecker _meetingParticipantPermittedChecker;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingOverviewView>();
            _meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
            _meetingParticipantPermittedChecker = _mocks.StrictMock<IMeetingParticipantPermittedChecker>();
            _target = new DeleteMeetingCommand(_view, _meetingRepository, _unitOfWorkFactory, _canModifyMeeting, _meetingParticipantPermittedChecker);
        }

		[Test]
		public void ShouldNotDoAnythingOnDeleteIfNoSelectedMeeting()
		{
			using (_mocks.Record())
			{
				Expect.Call(_canModifyMeeting.CanExecute).Return(true);
				Expect.Call(_view.SelectedMeeting).Return(null);
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void ShouldNotDoAnythingIfViewReturnsFalse()
		{
			var meeting = _mocks.StrictMock<IMeeting>();
			using (_mocks.Record())
			{
				Expect.Call(_view.SelectedMeeting).Return(meeting).Repeat.Twice();
				Expect.Call(_canModifyMeeting.CanExecute).Return(true);
				Expect.Call(_view.ConfirmDeletion(meeting)).Return(false);
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void ShouldCallRepositoryIfViewReturnsTrue()
		{
			var meeting = _mocks.StrictMock<IMeeting>();
			var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			using (_mocks.Record())
			{
				Expect.Call(_view.SelectedMeeting).Return(meeting).Repeat.Twice();
				Expect.Call(_canModifyMeeting.CanExecute).Return(true);
				Expect.Call(meeting.Snapshot);
				Expect.Call(_view.ConfirmDeletion(meeting)).Return(true);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(() => _meetingRepository.Remove(meeting));
				Expect.Call(() => unitOfWork.PersistAll());
				Expect.Call(_view.ReloadMeetings);
                Expect.Call(meeting.MeetingPersons).Return(new List<IMeetingPerson>());
                Expect.Call(meeting.StartDate).Return(new DateOnly());
                Expect.Call(() => unitOfWork.Reassociate(new List<IPerson>())).IgnoreArguments();
                Expect.Call(_meetingParticipantPermittedChecker.ValidatePermittedPersons(null, new DateOnly(), null, null)).
                    IgnoreArguments().Return(true);
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void CanExecuteShouldBeFalseIfNotAllowed()
		{
			using (_mocks.Record())
			{
				Expect.Call(_canModifyMeeting.CanExecute).Return(false);
			}
			using (_mocks.Playback())
			{
				Assert.That(_target.CanExecute(), Is.False);
			}
		}

        [Test]
        public void ShouldNotDeleteIfAnyParticipantNotPermitted()
        {
            var meeting = _mocks.StrictMock<IMeeting>();
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_view.SelectedMeeting).Return(meeting).Repeat.Twice();
                Expect.Call(meeting.MeetingPersons).Return(new List<IMeetingPerson>());
                Expect.Call(meeting.StartDate).Return(new DateOnly());
                Expect.Call(() => unitOfWork.Reassociate(new List<IPerson>())).IgnoreArguments();
                Expect.Call(_meetingParticipantPermittedChecker.ValidatePermittedPersons(null, new DateOnly(), null, null)).
                    IgnoreArguments().Return(false);
                Expect.Call(_canModifyMeeting.CanExecute).Return(true);
                Expect.Call(_view.ConfirmDeletion(meeting)).Return(true);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            }
            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}