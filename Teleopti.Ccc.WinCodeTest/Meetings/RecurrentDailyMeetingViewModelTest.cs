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
    public class RecurrentDailyMeetingViewModelTest
    {
        private RecurrentDailyMeetingViewModel _target;
        private IRecurrentDailyMeeting _recurrentDailyMeeting;
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
            _recurrentDailyMeeting = new RecurrentDailyMeeting {IncrementCount = 3};
            _target = new RecurrentDailyMeetingViewModel(_meetingViewModel, _recurrentDailyMeeting);
        }

        [Test]
        public void VerifyProperties()
        {
            bool eventFired = false;
            _target.PropertyChanged += (sender, e) =>
                                           {
                                               eventFired = true;
                                           };
            Assert.AreEqual(_recurrentDailyMeeting.IncrementCount, _target.IncrementCount);
            _target.IncrementCount = 4;
            Assert.AreEqual(4, _target.IncrementCount);
            Assert.AreEqual(RecurrentMeetingType.Daily,_target.RecurrentMeetingType);
            Assert.IsTrue(_target.IsValid());
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void VerifyCanSwitchToWeeklyRecurrentMeeting()
        {
            RecurrentWeeklyMeetingViewModel weeklyMeetingViewModel = _target.ChangeRecurringMeetingOption(RecurrentMeetingType.Weekly) as RecurrentWeeklyMeetingViewModel;
            Assert.IsNotNull(weeklyMeetingViewModel);
            Assert.AreEqual(3,weeklyMeetingViewModel.IncrementCount);
            Assert.IsTrue(weeklyMeetingViewModel[DayOfWeek.Wednesday]);
        }

        [Test]
        public void VerifyCanSwitchToMonthlyByDayRecurrentMeeting()
        {
            RecurrentMonthlyByDayMeetingViewModel monthlyByDayMeetingViewModel =
                _target.ChangeRecurringMeetingOption(RecurrentMeetingType.MonthlyByDay) as
                RecurrentMonthlyByDayMeetingViewModel;
            Assert.IsNotNull(monthlyByDayMeetingViewModel);
            Assert.AreEqual(3, monthlyByDayMeetingViewModel.IncrementCount);
            Assert.AreEqual(14, monthlyByDayMeetingViewModel.DayInMonth);
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
            RecurrentDailyMeetingViewModel dailyMeetingViewModel =
                _target.ChangeRecurringMeetingOption(RecurrentMeetingType.Daily) as RecurrentDailyMeetingViewModel;
            Assert.AreNotEqual(_target,dailyMeetingViewModel);
            Assert.AreEqual(_target.IncrementCount,dailyMeetingViewModel.IncrementCount);
        }
    }
}
