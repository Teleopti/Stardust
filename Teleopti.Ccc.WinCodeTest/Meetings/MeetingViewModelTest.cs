using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingViewModelTest
    {
        private IPerson _person;
        private IPerson _requiredPerson;
        private IPerson _optionalPerson;
        private IActivity _activity;
        private IScenario _scenario;
        private IMeeting _meeting;
        private CommonNameDescriptionSetting _commonNameSetting;
        private MeetingViewModel _target;
        private bool _propertyChanged;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("organizer", "1");
            _person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            
            _requiredPerson = PersonFactory.CreatePerson("required", "2");
            _optionalPerson = PersonFactory.CreatePerson("optional", "3");
            _activity = ActivityFactory.CreateActivity("Meeting");
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _meeting = new Meeting(_person,
                                   new List<IMeetingPerson>
                                       {
                                           new MeetingPerson(_requiredPerson, false),
                                           new MeetingPerson(_optionalPerson, true)
                                       }, "my subject", "my location",
                                   "my description", _activity, _scenario);
            _meeting.StartDate = new DateOnly(2009,10,14);
            _meeting.EndDate = new DateOnly(2009, 10, 16);
            _meeting.StartTime = TimeSpan.FromHours(23);
            _meeting.EndTime = TimeSpan.FromHours(25);
            _meeting.SetId(Guid.NewGuid());
            _commonNameSetting = new CommonNameDescriptionSetting("{LastName}, {FirstName}");

            _target = new MeetingViewModel(_meeting, _commonNameSetting);
            _target.PropertyChanged += (sender,e) => { _propertyChanged = true; };
        }

        [Test]
        public void VerifyOrganizer()
        {
            Assert.AreEqual("1, organizer", _target.Organizer);
        }

        [Test]
        public void VerifyParticipants()
        {
            Assert.AreEqual("2, required; 3, optional",_target.Participants);
            Assert.AreEqual(1,_target.RequiredParticipants.Count);
            Assert.AreEqual(1, _target.OptionalParticipants.Count);
        }

        [Test]
        public void VerifyDateAndTimeForMeeting()
        {
            Assert.AreEqual(_meeting.StartDate,_target.StartDate);
            Assert.AreEqual(_meeting.StartDate.AddDays(1), _target.EndDate);
            Assert.AreEqual(_meeting.EndDate, _target.RecurringEndDate);
            Assert.AreEqual(TimeSpan.FromHours(1), _target.EndTime);
            Assert.AreEqual(_meeting.StartTime, _target.StartTime);
            Assert.AreEqual(_meeting.MeetingDuration(),_target.MeetingDuration);
            Assert.AreEqual(_meeting,_target.OriginalMeeting);
            Assert.AreNotSame(_meeting, _target.Meeting);
            Assert.AreEqual(_meeting.Id,_target.Meeting.Id);

            _target.StartDate = _meeting.StartDate.AddDays(1);
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.StartDate, _target.StartDate);
            Assert.AreEqual(_target.Meeting.StartDate.AddDays(1), _target.EndDate);
            _propertyChanged = false;

            _target.RecurringEndDate = _target.Meeting.EndDate.AddDays(2);
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.EndDate, _target.RecurringEndDate);
            _propertyChanged = false;

            _target.StartTime = TimeSpan.FromHours(20);
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.StartTime, _target.StartTime);
            _propertyChanged = false;

            _target.MeetingDuration = TimeSpan.FromHours(2);
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.EndTime, _target.EndTime);
            _propertyChanged = false;

            _target.EndTime = TimeSpan.FromHours(2);
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.EndTime.Add(TimeSpan.FromDays(-1)), _target.EndTime);
            _propertyChanged = false;

            _target.RecurringEndDate = _target.Meeting.StartDate.AddDays(-2);
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.StartDate, _target.RecurringEndDate);
        }

        [Test]
        public void VerifyCannotCreateMeetingLongerThan24Hours()
        {
            _target.EndTime = TimeSpan.FromHours(22.5);
            Assert.AreEqual(TimeSpan.FromHours(23.5), _target.MeetingDuration);

            _target.StartTime = TimeSpan.FromHours(22);
            Assert.AreEqual(TimeSpan.FromMinutes(30),_target.MeetingDuration);
            Assert.AreEqual(TimeSpan.FromMinutes(30), _target.Meeting.MeetingDuration());
        }

        [Test]
        public void VerifyTextFields()
        {
            Assert.AreEqual(_meeting.Location,_target.Location);
            Assert.AreEqual(_meeting.Subject,_target.Subject);
            Assert.AreEqual(_meeting.Description,_target.Description);

            _target.Location = "New location";
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.Location, _target.Location);
            _propertyChanged = false;

            _target.Subject = "New subject";
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.Subject, _target.Subject);
            _propertyChanged = false;

            _target.Description = "New description";
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.Description, _target.Description);
        }

        [Test]
        public void VerifyActivityAndTimeZone()
        {
            Assert.AreEqual(_meeting.Activity,_target.Activity);
            Assert.AreEqual(_meeting.TimeZone,_target.TimeZone);

            IActivity newActivity = ActivityFactory.CreateActivity("Training");
            _target.Activity = newActivity;
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.Activity, _target.Activity);
            _propertyChanged = false;

            _target.TimeZone = new CccTimeZoneInfo(TimeZoneInfo.Utc);
            Assert.IsTrue(_propertyChanged);
            Assert.AreEqual(_target.Meeting.TimeZone, _target.TimeZone);
        }

        [Test]
        public void VerifyAddParticipants()
        {
            IPerson requiredParticipant = PersonFactory.CreatePerson("bart", "simpson");
            IPerson optionalParticipant = PersonFactory.CreatePerson("homer", "simpson");
            ContactPersonViewModel requiredPersonViewModel = new ContactPersonViewModel(requiredParticipant);
            ContactPersonViewModel optionalPersonViewModel = new ContactPersonViewModel(optionalParticipant);

            _target.AddParticipants(new List<ContactPersonViewModel> {requiredPersonViewModel},
                                    new List<ContactPersonViewModel> {optionalPersonViewModel});

            Assert.IsTrue(_target.Participants.Contains("simpson, bart"));
            Assert.IsTrue(_target.Participants.Contains("simpson, homer"));
        }

		[Test]
		public void ShouldSetAndGetSlotEndAndStartTime()
		{
			_target.SlotStartTime = TimeSpan.FromHours(7);
			_target.SlotEndTime = TimeSpan.FromHours(17);

			Assert.AreEqual(TimeSpan.FromHours(7),_target.SlotStartTime);
			Assert.AreEqual(TimeSpan.FromHours(17),_target.SlotEndTime);
		}
    }
}
