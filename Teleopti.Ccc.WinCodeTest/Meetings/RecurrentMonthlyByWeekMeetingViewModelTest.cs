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
    public class RecurrentMonthlyByWeekMeetingViewModelTest
    {
        private RecurrentMonthlyByWeekMeetingViewModel _target;
        private IRecurrentMonthlyByWeekMeeting _recurrentMonthlyByWeekMeeting;
        private IScenario _scenario;
        private IMeeting _meeting;
        private IPerson _person;
        private IPerson _requiredPerson;
        private IActivity _activity;
        private TimeZoneInfo _timeZone;
        private MeetingViewModel _meetingViewModel;

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
            _recurrentMonthlyByWeekMeeting = new RecurrentMonthlyByWeekMeeting { IncrementCount = 3 };
            _recurrentMonthlyByWeekMeeting.DayOfWeek = DayOfWeek.Friday;
            _recurrentMonthlyByWeekMeeting.WeekOfMonth = WeekNumber.Last;
            _target = new RecurrentMonthlyByWeekMeetingViewModel(_meetingViewModel, _recurrentMonthlyByWeekMeeting);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_recurrentMonthlyByWeekMeeting.IncrementCount,_target.IncrementCount);
            Assert.AreEqual(RecurrentMeetingType.MonthlyByWeek,_target.RecurrentMeetingType);
            Assert.AreEqual(DayOfWeek.Friday,_target.DayOfWeek);
            Assert.AreEqual(WeekNumber.Last, _target.WeekOfMonth);

            bool propertyChanged = false;
            _target.PropertyChanged += (sender, e) => { propertyChanged = true; };

            _target.DayOfWeek = DayOfWeek.Thursday;
            Assert.IsTrue(propertyChanged);
            propertyChanged = false;

            _target.WeekOfMonth = WeekNumber.Fourth;
            Assert.IsTrue(propertyChanged);

            Assert.AreEqual(DayOfWeek.Thursday, _target.DayOfWeek);
            Assert.AreEqual(WeekNumber.Fourth, _target.WeekOfMonth);
            Assert.IsTrue(_target.IsValid());
        }

        [Test]
        public void VerifyCanSwitchToDailyRecurrentMeeting()
        {
            RecurrentDailyMeetingViewModel dailyMeetingViewModel = _target.ChangeRecurringMeetingOption(RecurrentMeetingType.Daily) as RecurrentDailyMeetingViewModel;
            Assert.IsNotNull(dailyMeetingViewModel);
            Assert.AreEqual(3, dailyMeetingViewModel.IncrementCount);
        }

        [Test]
        public void VerifyCanSwitchToWeeklyRecurrentMeeting()
        {
            RecurrentWeeklyMeetingViewModel weeklyMeetingViewModel = _target.ChangeRecurringMeetingOption(RecurrentMeetingType.Weekly) as RecurrentWeeklyMeetingViewModel;
            Assert.IsNotNull(weeklyMeetingViewModel);
            Assert.AreEqual(3, weeklyMeetingViewModel.IncrementCount);
            Assert.IsTrue(weeklyMeetingViewModel[DayOfWeek.Wednesday]);
        }

        [Test]
        public void VerifyNothingIsDoneWhenTransformingToSameRecurrenceType()
        {
            RecurrentMonthlyByWeekMeetingViewModel monthlyByWeekMeetingViewModel =
                _target.ChangeRecurringMeetingOption(RecurrentMeetingType.MonthlyByWeek) as RecurrentMonthlyByWeekMeetingViewModel;
            Assert.AreNotEqual(_target, monthlyByWeekMeetingViewModel);
            Assert.AreEqual(_target.DayOfWeek, monthlyByWeekMeetingViewModel.DayOfWeek);
            Assert.AreEqual(_target.WeekOfMonth, monthlyByWeekMeetingViewModel.WeekOfMonth);
            Assert.AreEqual(_target.IncrementCount, monthlyByWeekMeetingViewModel.IncrementCount);
        }
    }
}
