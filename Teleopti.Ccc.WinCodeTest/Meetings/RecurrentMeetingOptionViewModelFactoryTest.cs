using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class RecurrentMeetingOptionViewModelFactoryTest
    {
        private IRecurrentDailyMeeting _recurrentDailyMeeting;
        private IRecurrentWeeklyMeeting _recurrentWeeklyMeeting;
        private IRecurrentMonthlyByDayMeeting _recurrentMonthlyByDayMeeting;
        private IScenario _scenario;
        private IMeeting _meeting;
        private IPerson _person;
        private IPerson _requiredPerson;
        private IActivity _activity;
        private TimeZoneInfo _timeZone;
        private MeetingViewModel _meetingViewModel;
        private RecurrentMonthlyByWeekMeeting _recurrentMonthlyByWeekMeeting;

        [SetUp]
        public void Setup()
        {
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

            _meetingViewModel = new MeetingViewModel(_meeting, new CommonNameDescriptionSetting());
            _recurrentDailyMeeting = new RecurrentDailyMeeting();
            _recurrentWeeklyMeeting = new RecurrentWeeklyMeeting();
            _recurrentMonthlyByDayMeeting = new RecurrentMonthlyByDayMeeting();
            _recurrentMonthlyByWeekMeeting = new RecurrentMonthlyByWeekMeeting();
        }

        [Test]
        public void VerifyFactoryWorks()
        {
            Assert.IsNotNull(RecurrentMeetingOptionViewModelFactory.CreateRecurrentMeetingOptionViewModel(_meetingViewModel,_recurrentDailyMeeting));
            Assert.IsNotNull(RecurrentMeetingOptionViewModelFactory.CreateRecurrentMeetingOptionViewModel(_meetingViewModel, _recurrentWeeklyMeeting));
            Assert.IsNotNull(RecurrentMeetingOptionViewModelFactory.CreateRecurrentMeetingOptionViewModel(_meetingViewModel, _recurrentMonthlyByDayMeeting));
            Assert.IsNotNull(RecurrentMeetingOptionViewModelFactory.CreateRecurrentMeetingOptionViewModel(_meetingViewModel, _recurrentMonthlyByWeekMeeting));
            Assert.IsNull(RecurrentMeetingOptionViewModelFactory.CreateRecurrentMeetingOptionViewModel(_meetingViewModel, null));
        }
    }
}
