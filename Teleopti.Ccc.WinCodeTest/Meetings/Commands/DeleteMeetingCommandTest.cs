using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Meetings.Commands;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingOverviewView>();
            _meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
            _target = new DeleteMeetingCommand(_view, _meetingRepository, _unitOfWorkFactory, _canModifyMeeting);
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
    }
}