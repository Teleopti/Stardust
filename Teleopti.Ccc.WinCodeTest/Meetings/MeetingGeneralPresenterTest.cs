using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class MeetingGeneralPresenterTest
    {
        private MockRepository _mocks;
        private IMeetingGeneralView _view;
        private MeetingViewModel _model;
        private MeetingGeneralPresenter _target;
        private IPerson _person;
        private IPerson _requiredPerson;
        private IPerson _optionalPerson;
        private IActivity _activity;
        private IScenario _scenario;
        private IMeeting _meeting;
        private IList<IActivity> _activityList;
        private ICccTimeZoneInfo _timeZone;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingGeneralView>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _person = PersonFactory.CreatePerson("organizer", "1");
            _requiredPerson = PersonFactory.CreatePerson("required", "2");
            _optionalPerson = PersonFactory.CreatePerson("optional", "3");
            _activity = ActivityFactory.CreateActivity("Meeting");
            _activityList = new List<IActivity> {ActivityFactory.CreateActivity("Training"), _activity};
            _timeZone = new CccTimeZoneInfo(TimeZoneInfo.Local);
            
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _meeting = new Meeting(_person,
                                   new List<IMeetingPerson>
                                       {
                                           new MeetingPerson(_requiredPerson, false),
                                           new MeetingPerson(_optionalPerson, true)
                                       }, "my subject", "my location",
                                   "my description", _activity, _scenario);
            _meeting.StartDate = new DateOnly(2009, 10, 14);
            _meeting.EndDate = new DateOnly(2009, 10, 16);
            _meeting.StartTime = TimeSpan.FromHours(19);
            _meeting.EndTime = TimeSpan.FromHours(21);
            _meeting.TimeZone = _timeZone;

            _model = new MeetingViewModel(_meeting, new CommonNameDescriptionSetting());
            _target = new MeetingGeneralPresenterForTest(_view, _model, _unitOfWorkFactory, _repositoryFactory);
        }

        [TearDown]
        public void Teardown()
        {
            _target.Dispose();
        }

        [Test]
        public void VerifyInitialize()
        {
            IActivityRepository activityRepository = _mocks.StrictMock<IActivityRepository>();
            IUnitOfWork unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateActivityRepository(unitOfWork)).Return(activityRepository);
            Expect.Call(activityRepository.LoadAllSortByName()).Return(_activityList);
            unitOfWork.Dispose();

            _view.SetOrganizer(_model.Organizer);
            _view.SetParticipants(_model.Participants);
            _view.SetActivityList(_activityList);
            _view.SetSelectedActivity(_model.Activity);
            _view.SetTimeZoneList(null);
            LastCall.IgnoreArguments();
            _view.SetSelectedTimeZone(_model.TimeZone);
            _view.SetStartDate(_model.StartDate);
            _view.SetEndDate(_model.EndDate);
            _view.SetStartTime(_model.StartTime);
            _view.SetEndTime(_model.EndTime);
            _view.SetRecurringEndDate(_model.RecurringEndDate);
            _view.SetSubject(_model.Subject);
            _view.SetLocation(_model.Location);
            _view.SetDescription(_model.Description);

            _mocks.ReplayAll();
            Assert.AreEqual(_model,_target.Model);
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetActivity()
        {
            _mocks.ReplayAll();
            _target.SetActivity(_activityList[0]);
            Assert.AreEqual(_activityList[0],_model.Activity);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetSubject()
        {
            _mocks.ReplayAll();
            _target.SetSubject("new subject");
            Assert.AreEqual("new subject", _model.Subject);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetLocation()
        {
            _mocks.ReplayAll();
            _target.SetLocation("new location");
            Assert.AreEqual("new location", _model.Location);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetDescription()
        {
            _mocks.ReplayAll();
            _target.SetDescription("new description");
            Assert.AreEqual("new description", _model.Description);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartDate()
        {
            _view.SetEndDate(new DateOnly(2009, 5, 1));
            _mocks.ReplayAll();
            _target.SetStartDate(new DateOnly(2009,5,1));
            Assert.AreEqual(new DateOnly(2009, 5, 1), _model.StartDate);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartDateForNonrecurring()
        {
            _model.RecurringEndDate = _model.StartDate;
            _view.SetEndDate(new DateOnly(2009, 5, 1));
            _view.SetRecurringEndDate(new DateOnly(2009,5,1));
            _mocks.ReplayAll();
            _target.SetStartDate(new DateOnly(2009, 5, 1));
            Assert.AreEqual(new DateOnly(2009, 5, 1), _model.StartDate);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartTime()
        {
            _view.SetEndDate(new DateOnly(2009, 10, 15));
            _mocks.ReplayAll();
            _target.SetStartTime(TimeSpan.FromHours(22));
            Assert.AreEqual(TimeSpan.FromHours(22), _model.StartTime);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartTimeAndEndTimeToTheSame()
        {
            _view.SetEndDate(new DateOnly(2009, 10, 15));
            _view.SetEndDate(new DateOnly(2009, 10, 15));
            _mocks.ReplayAll();
            _target.SetStartTime(TimeSpan.FromHours(22));
            _target.SetEndTime(TimeSpan.FromHours(22));
            Assert.AreEqual(TimeSpan.FromHours(22), _model.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(22), _model.EndTime);
            Assert.AreEqual(new DateOnly(2009, 10, 14),_model.StartDate);
            Assert.AreEqual(new DateOnly(2009, 10, 15), _model.EndDate);
            Assert.AreEqual(TimeSpan.FromDays(1),_model.Meeting.MeetingDuration());
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartTimeAndEndTimeToZero()
        {
            _view.SetEndDate(new DateOnly(2009, 10, 14));
            _view.SetEndDate(new DateOnly(2009, 10, 15));
            _mocks.ReplayAll();
            _target.SetStartTime(TimeSpan.FromHours(0));
            _target.SetEndTime(TimeSpan.FromHours(0));
            Assert.AreEqual(TimeSpan.FromHours(0), _model.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(0), _model.EndTime);
            Assert.AreEqual(new DateOnly(2009, 10, 14), _model.StartDate);
            Assert.AreEqual(new DateOnly(2009, 10, 15), _model.EndDate);
            Assert.AreEqual(TimeSpan.FromDays(1), _model.Meeting.MeetingDuration());
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetEndTimeAndStartTimeToZero()
        {
            _view.SetEndDate(new DateOnly(2009, 10, 15));
            _view.SetEndDate(new DateOnly(2009, 10, 15));
            _mocks.ReplayAll();
            _target.SetEndTime(TimeSpan.FromHours(0));
            _target.SetStartTime(TimeSpan.FromHours(0));
            Assert.AreEqual(TimeSpan.FromHours(0), _model.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(0), _model.EndTime);
            Assert.AreEqual(new DateOnly(2009, 10, 14), _model.StartDate);
            Assert.AreEqual(new DateOnly(2009, 10, 15), _model.EndDate);
            Assert.AreEqual(TimeSpan.FromDays(1), _model.Meeting.MeetingDuration());
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetEndTime()
        {
            _view.SetEndDate(new DateOnly(2009, 10, 14));
            _mocks.ReplayAll();
            _target.SetEndTime(TimeSpan.FromHours(21));
            Assert.AreEqual(TimeSpan.FromHours(21), _model.EndTime);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetParticipants()
        {
            _view.SetParticipants("");
            _meeting.ClearMeetingPersons();
            _mocks.ReplayAll();
            _target.ParseParticipants("required");
            Assert.IsTrue(string.IsNullOrEmpty(_model.Participants));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetParticipantsAndRemoveCorrectly()
        {
            _view.SetParticipants("");
            _meeting.ClearMeetingPersons();
            _mocks.ReplayAll();
            _target.ParseParticipants("reqrequired");
            Assert.IsTrue(string.IsNullOrEmpty(_model.Participants));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyParticipantsCanBeRefreshed()
        {
            _model.AddParticipants(new List<ContactPersonViewModel> {new ContactPersonViewModel(_optionalPerson)},
                                   new List<ContactPersonViewModel>());
            _view.SetParticipants(_model.Participants);
            _mocks.ReplayAll();
            _target.OnParticipantsSet();
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldUpdateStartTimeOnLeave()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _view.SetEndDate(_model.EndDate));
				Expect.Call(() => _view.NotifyMeetingTimeChanged());
			}

			using(_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeLeave("07:00");
			}
		}

		[Test]
		public void ShouldUpdateStartTimeOnLeaveWhenInputIsNotOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetStartTime(_model.StartTime));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeLeave("jobba på");
			}
		}

		[Test]
		public void ShouldUpdateStartTimeOnKeyDownEnter()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndDate(_model.EndDate));
				Expect.Call(() => _view.NotifyMeetingTimeChanged());
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeKeyDown(Keys.Enter, "07:00");
			}
		}

		[Test]
		public void ShouldNotUpdateStartTimeOnKeyDownNotEnter()
		{
			using (_mocks.Record())
			{
				//Not expecting anything
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeKeyDown(Keys.A, "07:00");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnLeave()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndDate(_model.EndDate));
				Expect.Call(() => _view.NotifyMeetingTimeChanged());
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeLeave("20:00");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnLeaveWhenInputIsNotOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndTime(_model.EndTime));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeLeave("jobba på");
			}
		}

		[Test]
		public void ShouldUpdateEndTimeOnKeyDownEnter()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndDate(_model.EndDate));
				Expect.Call(() => _view.NotifyMeetingTimeChanged());
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeKeyDown(Keys.Enter, "20:00");
			}
		}

		[Test]
		public void ShouldNotUpdateEndTimeOnKeyDownNotEnter()
		{
			using (_mocks.Record())
			{
				//Not expecting anything
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeKeyDown(Keys.A, "20:00");
			}
		}
    }

    public class MeetingGeneralPresenterForTest : MeetingGeneralPresenter
    {
        public MeetingGeneralPresenterForTest(IMeetingGeneralView view, MeetingViewModel model, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
            : base(view, model)
        {
            RepositoryFactory = repositoryFactory;
            UnitOfWorkFactory = unitOfWorkFactory;
        }
    }
}
