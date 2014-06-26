using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Meetings.Commands;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
	[TestFixture]
	public class EditMeetingCommandTest
	{
		private MockRepository _mocks;
		private IEditMeetingCommand _target;
		private IMeetingOverviewView _view;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private ISettingDataRepository _settingDataRepository;
		private ICanModifyMeeting _canModifyMeeting;
	    private IToggleManager _toggleManager;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.StrictMock<IMeetingOverviewView>();
			_settingDataRepository = _mocks.StrictMock<ISettingDataRepository>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
            _toggleManager = _mocks.StrictMock<IToggleManager>();
            _target = new EditMeetingCommand(_view, _settingDataRepository, _unitOfWorkFactory, _canModifyMeeting, _toggleManager);

		}

		[Test]
		public void ShouldJumpOutIfNoSelectedMeeting()
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
		public void ShouldCreateNewMeetingModelAndCallView()
		{
			var person = PersonFactory.CreatePerson();
			var meetingPerson = new MeetingPerson(person, false);
			var meeting = _mocks.StrictMock<IMeeting>();
			var meetingViewModel = _mocks.StrictMock<IMeetingViewModel>();
			var uow = _mocks.DynamicMock<IUnitOfWork>();

			using (_mocks.Record())
			{
				Expect.Call(_canModifyMeeting.CanExecute).Return(true);
				Expect.Call(_view.SelectedMeeting).Return(meeting).Repeat.Twice();
				Expect.Call(meeting.MeetingPersons).Return(
					new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> { meetingPerson })).Repeat.Any();
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);

				Expect.Call(_settingDataRepository.FindValueByKey("CommonNameDescription",
																  new CommonNameDescriptionSetting())).Return(
																	new CommonNameDescriptionSetting()).IgnoreArguments();
				Expect.Call(() => uow.Reassociate(new List<IPerson> { person }));
				Expect.Call(meeting.EntityClone()).Return(meeting);
				Expect.Call(meeting.MeetingRecurrenceOption).Return(null);
				Expect.Call(meeting.Snapshot);
				Expect.Call(() => _view.EditMeeting(meetingViewModel, _toggleManager)).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void CanExecuteShouldBeFalseIfNoMeetingSelected()
		{
			using (_mocks.Record())
			{
				Expect.Call(_canModifyMeeting.CanExecute).Return(true);
				Expect.Call(_view.SelectedMeeting).Return(null);
			}
			using (_mocks.Playback())
			{
				Assert.That(_target.CanExecute(), Is.False);
			}
		}

		[Test]
		public void CanExecuteShouldBeTrueIfAMeetingIsSelected()
		{
			var meeting = _mocks.StrictMock<IMeeting>();

			using (_mocks.Record())
			{
				Expect.Call(_canModifyMeeting.CanExecute).Return(true);
				Expect.Call(_view.SelectedMeeting).Return(meeting);
			}
			using (_mocks.Playback())
			{
				Assert.That(_target.CanExecute(), Is.True);
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