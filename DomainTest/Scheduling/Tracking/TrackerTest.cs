using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;


namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
    [TestFixture]
    public class TrackerTest
    {
        private ITracker _dayTracker;
        private ITracker _timeTracker;
        private ITracker _compTracker;
        private ITracker _overTimeTracker;
        private TestTrackerOne _trackOne;
        private TestTrackerTwo _trackTwo;
        private MockRepository _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _dayTracker = Tracker.CreateDayTracker();
            _timeTracker = Tracker.CreateTimeTracker();
            _compTracker = Tracker.CreateCompTracker();
            _overTimeTracker = Tracker.CreateOvertimeTracker();
        }

        [Test]
        public void VerifyTrackerFactories()
        {

            Assert.AreEqual(_dayTracker, Tracker.CreateDayTracker());
            Assert.AreEqual(_timeTracker, Tracker.CreateTimeTracker());
            Assert.AreEqual(_compTracker, Tracker.CreateCompTracker());
            Assert.AreEqual(_overTimeTracker, Tracker.CreateOvertimeTracker());
        }

        [Test]
        public void VerifyCanGetListOfAllAvailableTrackers()
        {
            Assert.IsTrue(Tracker.AllTrackers().Contains(_dayTracker));
            Assert.IsTrue(Tracker.AllTrackers().Contains(_timeTracker));
            Assert.AreEqual(2, Tracker.AllTrackers().Count);
        }

        [Test]
        public void VerifyDescriptions()
        {
            Assert.AreEqual(_dayTracker.Description.Name, UserTexts.Resources.HolidayDays);
            Assert.AreEqual(_timeTracker.Description.Name, UserTexts.Resources.HolidayTime);
            Assert.AreEqual(_compTracker.Description.Name, UserTexts.Resources.CompTime);
            Assert.AreEqual(_overTimeTracker.Description.Name, UserTexts.Resources.Overtime);
        }

        [Test]
        public void VerifyEqualsIsBasedOnType()
        {
            _trackOne = new TestTrackerOne();
            _trackTwo = new TestTrackerTwo();
            Assert.AreNotEqual(_trackOne,_trackTwo);
            Assert.AreEqual(_trackOne, _trackOne);

            Assert.AreEqual(false, _trackOne.Equals(null));
        } 

        [Test]
        public void VerifyHashCode()
        {
            _trackOne = new TestTrackerOne();
            IDictionary<ITracker, int> dic = new Dictionary<ITracker, int>();
            dic[_trackOne] = 3;
            Assert.IsTrue(dic.ContainsKey(new TestTrackerOne()));
            Assert.IsFalse(dic.ContainsKey(new TestTrackerTwo()));

        }

        [Test]
        public void TestDayTrackerCallsCorrectCalculatorMethodOnTraceableUsingListOfDays()
        {

            var traceable = _mocker.StrictMock<ITraceable>();
            var part = _mocker.StrictMock<IScheduleDay>();
            var part2 = _mocker.StrictMock<IScheduleDay>();
            IList<IScheduleDay> days = new List<IScheduleDay> { part, part2 };
            var account = _mocker.StrictMock<IAccount>();
            var calculator = _mocker.StrictMock<ITrackerCalculator>();
            IAbsence absenceToTrack = new Absence();
            var result = TimeSpan.FromDays(12);
            var owner = new PersonAbsenceAccount(new Person(), absenceToTrack);


            ((Tracker)_dayTracker).InjectCalculator(calculator);
            using (_mocker.Record())
            {
                Expect.Call(account.Owner).Return(owner);
                Expect.Call(() => traceable.Track(result));
                Expect.Call(calculator.CalculateNumberOfDaysOnScheduleDays(absenceToTrack, days)).Return(result);
            }
            using (_mocker.Playback())
            {
                _dayTracker.Track(traceable, account.Owner.Absence, days);
            }
        }

       
        [Test]
        public void TestTimeTrackerCallsCorrectCalculatorMethodOnTraceableUsingListOfDays()
        {

            var traceable = _mocker.StrictMock<ITraceable>();
            var part = _mocker.StrictMock<IScheduleDay>();
            var part2 = _mocker.StrictMock<IScheduleDay>();
            IList<IScheduleDay> days = new List<IScheduleDay> { part, part2 };
            var account = _mocker.StrictMock<IAccount>();
            var calculator = _mocker.StrictMock<ITrackerCalculator>();
            IAbsence absenceToTrack = new Absence();
            TimeSpan result = TimeSpan.FromMinutes(12);
            var owner = new PersonAbsenceAccount(new Person(), absenceToTrack);

            ((Tracker)_timeTracker).InjectCalculator(calculator);
            using (_mocker.Record())
            {

                Expect.Call(account.Owner).Return(owner);
                Expect.Call(() => traceable.Track(result));
                Expect.Call(calculator.CalculateTotalTimeOnScheduleDays(absenceToTrack, days)).Return(result);
            }
            using (_mocker.Playback())
            {
                _timeTracker.Track(traceable, account.Owner.Absence, days);
            }
        }

        [Test]
        public void CanCreateCorrectPersonAccount()
        {
            IAccount accountTime =
                _timeTracker.CreatePersonAccount(new DateOnly(2009, 3, 2));
            
            Assert.IsNotNull(accountTime);

            IAccount accountDay =
                _dayTracker.CreatePersonAccount(new DateOnly(2009, 3, 2));

            Assert.IsNotNull(accountDay);

            IAccount accountCompTime =
                _compTracker.CreatePersonAccount(new DateOnly(2009, 3, 2));

            Assert.IsNotNull(accountCompTime);

            IAccount accountOverTime =
              _overTimeTracker.CreatePersonAccount(new DateOnly(2009, 3, 2));

            Assert.IsNotNull(accountOverTime);
        }



       
        #region Comp TODO
      
        [Test]
        public void TimeActivityLayerTrackingOnProjection()
        {
            var traceable = _mocker.StrictMock<ITraceable>();
            var part = _mocker.StrictMock<IScheduleDay>();
            var part2 = _mocker.StrictMock<IScheduleDay>();
            IList<IScheduleDay> days = new List<IScheduleDay> { part, part2 };
            var trackingAbsence = _mocker.StrictMock<IAbsence>();

            _compTracker.Track(traceable, trackingAbsence, days);
        }
        #endregion //Comp TODO

        #region OverTime 
        
        [Test]
        public void OvertimeTrackingOnScheduleDays()
        {
            var traceable = _mocker.StrictMock<ITraceable>();
            var part = _mocker.StrictMock<IScheduleDay>();
            var part2 = _mocker.StrictMock<IScheduleDay>();
           IList<IScheduleDay> days = new List<IScheduleDay>{part,part2};
            var trackingAbsence = _mocker.StrictMock<IAbsence>();

            _overTimeTracker.Track(traceable, trackingAbsence, days);
        }
        #endregion 


        private class TestTrackerTwo : TestTrackerOne
        {
            public override Description Description
            {
                get { return new Description("Tracker Two"); }
            }

        }

    }
}
