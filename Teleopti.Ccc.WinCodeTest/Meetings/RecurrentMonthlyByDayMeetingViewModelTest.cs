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
    public class RecurrentMonthlyByDayMeetingViewModelTest
    {
        private RecurrentMonthlyByDayMeetingViewModel _target;
        private IRecurrentMonthlyByDayMeeting _recurrentMonthlyByDayMeeting;
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
            _recurrentMonthlyByDayMeeting = new RecurrentMonthlyByDayMeeting {IncrementCount = 3};
            _recurrentMonthlyByDayMeeting.DayInMonth = 14;
            _target = new RecurrentMonthlyByDayMeetingViewModel(_meetingViewModel, _recurrentMonthlyByDayMeeting);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_recurrentMonthlyByDayMeeting.IncrementCount,_target.IncrementCount);
            Assert.AreEqual(RecurrentMeetingType.MonthlyByDay,_target.RecurrentMeetingType);
            Assert.AreEqual(14,_target.DayInMonth);
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
        public void VerifyCanSwitchToMonthlyByWeekRecurrentMeeting()
        {
            RecurrentMonthlyByWeekMeetingViewModel monthlyByWeekMeetingViewModel =
                _target.ChangeRecurringMeetingOption(RecurrentMeetingType.MonthlyByWeek) as RecurrentMonthlyByWeekMeetingViewModel;
            Assert.IsNotNull(monthlyByWeekMeetingViewModel);
            Assert.AreEqual(3, monthlyByWeekMeetingViewModel.IncrementCount);
            Assert.AreEqual(DayOfWeek.Wednesday, monthlyByWeekMeetingViewModel.DayOfWeek);
            Assert.AreEqual(WeekNumber.First, monthlyByWeekMeetingViewModel.WeekOfMonth);
        }

        [Test]
        public void VerifyNothingIsDoneWhenTransformingToSameRecurrenceType()
        {
            RecurrentMonthlyByDayMeetingViewModel monthlyByDayMeetingViewModel =
                _target.ChangeRecurringMeetingOption(RecurrentMeetingType.MonthlyByDay) as RecurrentMonthlyByDayMeetingViewModel;
            Assert.AreNotEqual(_target,monthlyByDayMeetingViewModel);
            Assert.AreEqual(_target.DayInMonth, monthlyByDayMeetingViewModel.DayInMonth);
            Assert.AreEqual(_target.IncrementCount, monthlyByDayMeetingViewModel.IncrementCount);
        }
    }
}
