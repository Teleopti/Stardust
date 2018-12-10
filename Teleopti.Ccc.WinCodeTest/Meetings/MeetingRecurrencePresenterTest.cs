using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingRecurrencePresenterTest
    {
        private IPerson _person;
        private IPerson _requiredPerson;
        private IScenario _scenario;
        private IActivity _activity;
        private TimeZoneInfo _timeZone;
        private IMeeting _meeting;
        private MeetingViewModel _meetingViewModel;
        private MeetingRecurrencePresenter _target;
        private MockRepository _mocks;
        private IMeetingRecurrenceView _view;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person = PersonFactory.CreatePerson("organizer", "1");
            _requiredPerson = PersonFactory.CreatePerson("required", "2");
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _activity = ActivityFactory.CreateActivity("Meeting");
            _timeZone = (TimeZoneInfo.Local);
            _meeting = new Meeting(_person,
                                   new List<IMeetingPerson>
                                       {
                                           new MeetingPerson(_requiredPerson, false),
                                       }, "my subject", "my location",
                                   "my description", _activity, _scenario);
            _meeting.StartDate = new DateOnly(2009, 10, 14);
            _meeting.EndDate = new DateOnly(2009, 10, 16);
            _meeting.StartTime = TimeSpan.FromHours(19);
            _meeting.EndTime = TimeSpan.FromHours(21);
            _meeting.TimeZone = _timeZone;
            RecurrentWeeklyMeeting recurrentWeeklyMeeting = new RecurrentWeeklyMeeting();
            recurrentWeeklyMeeting[DayOfWeek.Wednesday] = true;
            _meeting.SetRecurrentOption(recurrentWeeklyMeeting);

            _view = _mocks.StrictMock<IMeetingRecurrenceView>();
            _meetingViewModel = new MeetingViewModel(_meeting, new CommonNameDescriptionSetting());
            _target = new MeetingRecurrencePresenter(_view, _meetingViewModel);
        }

        [Test]
        public void VerifyCanInitialize()
        {
            CreateInitializationExpectation();

            _mocks.ReplayAll();
            _target.Initialize();
            _mocks.VerifyAll();
        }

        private void CreateInitializationExpectation()
        {
            _view.SetStartTime(_meetingViewModel.StartTime);
            _view.SetEndTime(_meetingViewModel.EndTime);
            _view.SetStartDate(_meetingViewModel.StartDate);
            _view.SetRecurringEndDate(_meetingViewModel.RecurringEndDate);
            _view.SetRecurringOption(_meetingViewModel.RecurringOption.RecurrentMeetingType);
            _view.SetRecurringExists(true);
            LastCall.IgnoreArguments(); //We will actually get a copy of the recurring option as we aren't sure that we'll use the changes
        }

        [Test]
        public void VerifyCanRemoveRecurrence()
        {
            _view.Close();
            _meeting.SetRecurrentOption(new RecurrentWeeklyMeeting{IncrementCount = 3});

            _mocks.ReplayAll();
            _target.RemoveRecurrence();
            Assert.IsInstanceOf(typeof(IRecurrentDailyMeeting),_meetingViewModel.Meeting.MeetingRecurrenceOption);
            Assert.AreEqual(1, _meetingViewModel.Meeting.MeetingRecurrenceOption.IncrementCount);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSaveRecurrenceToMeeting()
        {
            CreateInitializationExpectation();

            _view.RefreshRecurrenceOption(null);
            LastCall.IgnoreArguments();
            _view.AcceptAndClose();

            _mocks.ReplayAll();

            DateOnly newRecurringEndDate = _meetingViewModel.RecurringEndDate.AddDays(5);
            _target.Initialize();
            _target.ChangeRecurringType(RecurrentMeetingType.Weekly);
            _target.SetRecurringEndDate(newRecurringEndDate);
            _target.SaveRecurrenceForMeeting();

            Assert.AreEqual(newRecurringEndDate,_meetingViewModel.RecurringEndDate);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCannotSaveRecurrenceToMeetingWithRecurringEndDateSmallerThanStartDate()
        {
            CreateInitializationExpectation();

            _view.RefreshRecurrenceOption(null);
            LastCall.IgnoreArguments();
            _view.ShowErrorMessage("","");
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            _target.Initialize();
            _target.ChangeRecurringType(RecurrentMeetingType.Weekly);
            _target.SetRecurringEndDate(_meetingViewModel.RecurringEndDate.AddDays(-5));
            _target.SaveRecurrenceForMeeting();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartDate()
        {
            CreateInitializationExpectation();
            _view.SetRecurringEndDate(_meetingViewModel.StartDate.AddDays(5));

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetStartDate(_meetingViewModel.StartDate.AddDays(5));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartTime()
        {
            CreateInitializationExpectation();
            _view.SetStartTime(TimeSpan.FromHours(20));
            _view.SetStartTime(TimeSpan.FromHours(22));

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetStartTime(TimeSpan.FromHours(20));
            _target.SetStartTime(TimeSpan.FromHours(22));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetEndTime()
        {
            CreateInitializationExpectation();
            _view.SetEndTime(TimeSpan.FromHours(20));
            _view.SetEndTime(TimeSpan.FromHours(18));

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetEndTime(TimeSpan.FromHours(20));
            _target.SetEndTime(TimeSpan.FromHours(18));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetDuration()
        {
            CreateInitializationExpectation();
            _view.SetEndTime(TimeSpan.FromHours(22));
            _view.SetEndTime(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(59)));

            _mocks.ReplayAll();
            _target.Initialize();
            _target.SetMeetingDuration(TimeSpan.FromHours(3));
            _target.SetMeetingDuration(TimeSpan.FromHours(25));
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldGetStartTime()
		{
			_meetingViewModel.StartTime = TimeSpan.FromHours(8);

			using (_mocks.Record())
			{
				CreateInitializationExpectation();
			}

			using (_mocks.Playback())
			{
				_target.Initialize();
				Assert.AreEqual(TimeSpan.FromHours(8), _target.GetStartTime);
			}
		}

		[Test]
		public void ShouldGetEndTime()
		{
			_meetingViewModel.EndTime = TimeSpan.FromHours(17);

			using (_mocks.Record())
			{
				CreateInitializationExpectation();
			}

			using (_mocks.Playback())
			{
				_target.Initialize();
				Assert.AreEqual(TimeSpan.FromHours(21), _target.GetEndTime);
			}
		}

		[Test]
		public void ShouldGetDurationTime()
		{
			_meetingViewModel.MeetingDuration = TimeSpan.FromHours(1);

			using (_mocks.Record())
			{
				CreateInitializationExpectation();
			}

			using (_mocks.Playback())
			{
				_target.Initialize();
				Assert.AreEqual(TimeSpan.FromHours(1), _target.GetDurationTime);
			}
		}

		[Test]
		public void ShouldUpdateStartTimeOnSelectedStartTimeLeaveOnInputOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(()=>_view.SetStartTime(TimeSpan.FromHours(7)));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartLeave("07:00");
			}
		}

		[Test]
		public void ShouldUpdateStartTimeOnSelectedStartTimeLeaveOnInputNotOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetStartTime(TimeSpan.FromHours(19)));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartLeave("jobba");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnSelectedEndTimeLeaveOnInputOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndTime(TimeSpan.FromHours(17)));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndLeave("17:00");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnSelectedEndTimeLeaveOnInputNotOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndTime(TimeSpan.FromHours(21))).IgnoreArguments();
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndLeave("jobba");
			}
		}

		[Test]
		public void ShouldUpdateStartTimeOnSelectedStartTimeEnterKeyOnInputOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetStartTime(TimeSpan.FromHours(7)));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartKeyDown(Keys.Enter, "07:00");
			}
		}

		[Test]
		public void ShouldUpdateStartTimeOnSelectedStartTimeEnterKeyOnInputNotOk()
		{
			using (_mocks.Record())
			{
				//not expecting anything
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartKeyDown(Keys.A, "07:00");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnSelectedEndTimeEnterKeyOnInputOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndTime(TimeSpan.FromHours(17)));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndKeyDown(Keys.Enter, "17:00");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnSelectedEndTimeEnterKeyOnInputNotOk()
		{
			using (_mocks.Record())
			{
				//Not expecting anything
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndKeyDown(Keys.A, "17:00");
			}
		}

		[Test]
		public void ShouldUpdateStartTimeOnSelectedStartTimeOnSelectedIndexChanged()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetStartTime(TimeSpan.FromHours(7)));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartSelectedIndexChanged("07:00");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnSelectedEndTimeOnSelectedIndexChanged()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndTime(TimeSpan.FromHours(17)));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndSelectedIndexChanged("17:00");
			}
		}

    }
}
