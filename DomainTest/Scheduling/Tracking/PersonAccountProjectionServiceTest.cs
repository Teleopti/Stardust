using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
    [TestFixture]
    public class PersonAccountProjectionServiceTest
    {
        private MockRepository _mocker;
        private IAccount _account;
        private IPerson _person;
        private IScheduleRange _schedule;
        private DateTime _dateTime1;
        private DateTime _dateTime2;
        private DateTime _dateTime3;
        private DateTime _dateTime4;
        private IPersonAccountProjectionService _target;

        private List<DateTimePeriod> _periodsToReadFromDatabase;
        private DateOnlyPeriod _accountPeriod;
        private DateTimePeriod _schedulePeriod;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _person = PersonFactory.CreatePerson();
            _account = _mocker.StrictMock<IAccount>();
            _schedule = _mocker.StrictMock<IScheduleRange>();
            _dateTime1 = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _dateTime2 = _dateTime1.Add(TimeSpan.FromDays(100));
            _dateTime3 = _dateTime2.Add(TimeSpan.FromDays(100));
            _dateTime4 = _dateTime3.Add(TimeSpan.FromDays(100));
        }

        private void SetupExpects()
        {
            using (_mocker.Record())
            {
                Expect.Call(_account.Period()).Return(_accountPeriod).Repeat.Any();
                Expect.Call(_schedule.Period).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_schedule.Person).Return(_person).Repeat.Any();
                Expect.Call(_account.Parent).Return(_person).Repeat.Any();
            }
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyThatPersonMustBeTheSameOnAccountAndSchedule()
        {
            IPerson person1 = new Person();
            IPerson person2 = new Person();
            using (_mocker.Record())
            {
                Expect.Call(_account.Owner).Return(new PersonAbsenceAccount(person1, new Absence()));
                Expect.Call(_account.Period()).Return(_accountPeriod).Repeat.Any();
                Expect.Call(_schedule.Period).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(_account.Parent).Return(person1);
                Expect.Call(_schedule.Person).Return(person2);
            }
            using (_mocker.Playback())
            {
                _target = new PersonAccountProjectionService(_account, _schedule);
            }
        }

        [Test]
        public void VerifyGetsTheCorrectPeriodsIfLoadedScheduleIsInsidePersonAccount()
        {
            var period = new DateTimePeriod(_dateTime1, _dateTime4);
            var timeZone = _person.PermissionInformation.DefaultTimeZone();
            _accountPeriod = period.ToDateOnlyPeriod(timeZone);
            _schedulePeriod = new DateTimePeriod(_dateTime2, _dateTime3);
            DateTimePeriod tempAccountPeriod = _accountPeriod.ToDateTimePeriod(timeZone);
            var owner = new PersonAbsenceAccount(_person, new Absence());
            Expect.Call(_account.Owner).Return(owner).Repeat.Any();
            SetupExpects();

            using (_mocker.Playback())
            {
                _target = new PersonAccountProjectionService(_account, _schedule);
                _periodsToReadFromDatabase = (List<DateTimePeriod>)_target.PeriodsToLoad();
                Assert.AreEqual(2, _periodsToReadFromDatabase.Count);
                Assert.Contains(new DateTimePeriod(tempAccountPeriod.StartDateTime, _dateTime2), _periodsToReadFromDatabase);
                Assert.Contains(new DateTimePeriod(_dateTime3, tempAccountPeriod.EndDateTime), _periodsToReadFromDatabase);
                Assert.AreEqual(new DateTimePeriod(_dateTime2, _dateTime3), _target.PeriodToReadFromSchedule());
            }
        }

        [Test]
        public void VerifyGetsTheCorrectPeriodsIfLoadedScheduleCoversThePersonAccount()
        {
            var period = new DateTimePeriod(_dateTime2, _dateTime3);
            var timeZone = _person.PermissionInformation.DefaultTimeZone();
            _accountPeriod = period.ToDateOnlyPeriod(timeZone);
            _schedulePeriod = new DateTimePeriod(_dateTime1, _dateTime4);
            var owner = new PersonAbsenceAccount(_person, new Absence());
            Expect.Call(_account.Owner).Return(owner).Repeat.Any();
            SetupExpects();

            using (_mocker.Playback())
            {
                _target = new PersonAccountProjectionService(_account, _schedule);
                _periodsToReadFromDatabase = (List<DateTimePeriod>)_target.PeriodsToLoad();
                Assert.AreEqual(0, _periodsToReadFromDatabase.Count);
                Assert.AreEqual(_accountPeriod.ToDateTimePeriod(timeZone), _target.PeriodToReadFromSchedule());
            }
        }

        [Test]
        public void VerifyGetsPersonAccountPeriodIfLoadedScheduleIsOutsideThePersonAccountOrNull()
        {
            var timeZone = _person.PermissionInformation.DefaultTimeZone();
            var period = new DateTimePeriod(_dateTime1, _dateTime2);
            _accountPeriod = period.ToDateOnlyPeriod(timeZone);
            _schedulePeriod = new DateTimePeriod(_dateTime3, _dateTime4);
            var owner = new PersonAbsenceAccount(_person, new Absence());
            Expect.Call(_account.Owner).Return(owner).Repeat.Any();
            SetupExpects();

            using (_mocker.Playback())
            {
                _target = new PersonAccountProjectionService(_account, _schedule);
                _periodsToReadFromDatabase = (List<DateTimePeriod>)_target.PeriodsToLoad();
                Assert.AreEqual(_accountPeriod.ToDateTimePeriod(timeZone), _periodsToReadFromDatabase[0]);
                Assert.IsNull(_target.PeriodToReadFromSchedule());

                _target = new PersonAccountProjectionService(_account, null);
                _periodsToReadFromDatabase = (List<DateTimePeriod>)_target.PeriodsToLoad();
                Assert.AreEqual(_accountPeriod.ToDateTimePeriod(timeZone), _periodsToReadFromDatabase[0]);
                Assert.IsNull(_target.PeriodToReadFromSchedule());
            }
        }

        [Test]
        public void VerifyGetsPersonAccountPeriodIfLoadedScheduleIsPartlyOutsideThePersonAccountStart()
        {
            var period = new DateTimePeriod(_dateTime2, _dateTime4);
            var timeZone = _person.PermissionInformation.DefaultTimeZone();
            _accountPeriod = period.ToDateOnlyPeriod(timeZone);
            _schedulePeriod = new DateTimePeriod(_dateTime1, _dateTime3);
            DateTimePeriod tempAccountPeriod = _accountPeriod.ToDateTimePeriod(timeZone);
            var owner = new PersonAbsenceAccount(_person, new Absence());
            Expect.Call(_account.Owner).Return(owner).Repeat.Any();

            SetupExpects();
            using (_mocker.Playback())
            {
                _target = new PersonAccountProjectionService(_account, _schedule);
                _periodsToReadFromDatabase = (List<DateTimePeriod>)_target.PeriodsToLoad();
                Assert.AreEqual(new DateTimePeriod(_dateTime3, tempAccountPeriod.EndDateTime),
                                _periodsToReadFromDatabase[0]);
                Assert.AreEqual(
                    new DateTimePeriod(tempAccountPeriod.StartDateTime, _dateTime3), _target.PeriodToReadFromSchedule());
            }
        }

        [Test]
        public void VerifyGetsPersonAccountPeriodIfLoadedScheduleIsPartlyOutsideThePersonAccountEnd()
        {
            var timeZone = _person.PermissionInformation.DefaultTimeZone();
            var period = new DateTimePeriod(_dateTime1, _dateTime3);
            _accountPeriod = period.ToDateOnlyPeriod(timeZone);
            _schedulePeriod = new DateTimePeriod(_dateTime2, _dateTime4);
            DateTimePeriod tempAccountPeriod = _accountPeriod.ToDateTimePeriod(timeZone);
            var owner = new PersonAbsenceAccount(_person, new Absence());
            Expect.Call(_account.Owner).Return(owner).Repeat.Any();
            SetupExpects();

            using (_mocker.Playback())
            {
                _target = new PersonAccountProjectionService(_account, _schedule);
                _periodsToReadFromDatabase = (List<DateTimePeriod>)_target.PeriodsToLoad();
                Assert.AreEqual(new DateTimePeriod(tempAccountPeriod.StartDateTime, _dateTime2),
                                _periodsToReadFromDatabase[0]);
                Assert.AreEqual(new DateTimePeriod(_dateTime2, tempAccountPeriod.EndDateTime),
                                _target.PeriodToReadFromSchedule());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyNumberOfDays()
        {
            var period = new DateTimePeriod(_dateTime2, _dateTime4);
            var timeZone = _person.PermissionInformation.DefaultTimeZone();
            _accountPeriod = period.ToDateOnlyPeriod(timeZone);
            _schedulePeriod = new DateTimePeriod(_dateTime2, _dateTime3);
            var schedulePeriodDateOnly = _schedulePeriod.ToDateOnlyPeriod(timeZone);
            _schedulePeriod = schedulePeriodDateOnly.ToDateTimePeriod(timeZone);
            
            IAbsence absence = AbsenceFactory.CreateAbsence("for test"); 
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();

            IScheduleStorage storage = _mocker.StrictMock<IScheduleStorage>();
            IScheduleRange rangeFromRepository = _mocker.StrictMock<IScheduleRange>();
            
            var tempAccountPeriod = _accountPeriod.ToDateTimePeriod(timeZone);
            var owner = new PersonAbsenceAccount(_person, absence);
			var day = _mocker.StrictMock<IScheduleDay>();
            using (_mocker.Record())
            {
                Expect.Call(_account.Parent).Return(_person).Repeat.Any();
                Expect.Call(_schedule.Person).Return(_person).Repeat.Any();
                Expect.Call(_account.Owner).Return(owner).Repeat.Any();
                Expect.Call(_account.Period()).Return(_accountPeriod).Repeat.Any();
                Expect.Call(_schedule.Period).Return(_schedulePeriod).Repeat.Any();
                Expect.Call(
                    storage.ScheduleRangeBasedOnAbsence(
                        new DateTimePeriod(_schedulePeriod.EndDateTime, tempAccountPeriod.EndDateTime), scenario,
                        _person, absence)).Return(rangeFromRepository);
				Expect.Call(day.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_accountPeriod.StartDate, _person.PermissionInformation.DefaultTimeZone()));
				Expect.Call(day.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_accountPeriod.StartDate.AddDays(1), _person.PermissionInformation.DefaultTimeZone()));

                //Days from repository:
				Expect.Call(rangeFromRepository.ScheduledDayCollection(new DateOnlyPeriod(2001,7,20,2001,10,27))).Return(new[] { day });

                //Days from schedule:
				Expect.Call(_schedule.ScheduledDayCollection(new DateOnlyPeriod(2001,4,11,2001,7,19))).Return(new[] { day });
            }

            using (_mocker.Playback())
            {
                _target = new PersonAccountProjectionService(_account, _schedule);
                var days = _target.CreateProjection(storage, scenario);

                //Verify correct number of days is returned
                Assert.AreEqual(2, days.Count);
            }
        }
    }
}
