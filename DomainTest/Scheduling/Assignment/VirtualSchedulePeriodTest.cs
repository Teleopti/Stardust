using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class VirtualSchedulePeriodTest
    {
        private VirtualSchedulePeriod _target;
        private IPerson _person;
        private DateOnly _dateOnly;
        private ISchedulePeriod _schedulePeriod;
        private IPersonPeriod _personPeriod;
        private IList<ISchedulePeriod> _schedulePeriods;
        private IPersonContract _personContract;
        private IContractSchedule _contractSchedule;
        private ITeam _team;
        private ISite _site;
        private IVirtualSchedulePeriodSplitChecker _splitChecker;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _person = new Person();
			_dateOnly = new DateOnly(2009, 2, 2);
            IContract contract = ContractFactory.CreateContract("MyContract");
            IPartTimePercentage partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
            _contractSchedule = ContractScheduleFactory.CreateContractSchedule("Mon-Sat");
            IContractScheduleWeek week1 = new ContractScheduleWeek();
            week1.Add(DayOfWeek.Monday, true);
            week1.Add(DayOfWeek.Tuesday, true);
            week1.Add(DayOfWeek.Wednesday, true);
            week1.Add(DayOfWeek.Thursday, true);
            week1.Add(DayOfWeek.Friday, true);
            _contractSchedule.AddContractScheduleWeek(week1);
            _personContract = new PersonContract(contract, partTime, _contractSchedule);
            // simple case, the schedulePeriod and personperiod starts same date we ask for
            _schedulePeriod = new SchedulePeriod(_dateOnly, SchedulePeriodType.Week, 4);
            _schedulePeriod.BalanceIn = TimeSpan.FromHours(5);
            _schedulePeriod.BalanceOut = TimeSpan.FromHours(6);
            _schedulePeriod.Extra = TimeSpan.FromHours(7);
            _schedulePeriod.Seasonality = new Percent(0.5);

            _schedulePeriod.SetParent(_person);

            _schedulePeriods = new List<ISchedulePeriod> { _schedulePeriod };
            _team = new Team();
            _site = new Site("Site");
            _site.AddTeam(_team);

            _personPeriod = new PersonPeriod(_dateOnly, _personContract, _team);
            IRuleSetBag ruleSetBag = new RuleSetBag();
            _personPeriod.RuleSetBag = ruleSetBag;
            _person.ActivatePerson(new MockRepository().StrictMock<IPersonAccountUpdater>());
            _person.AddPersonPeriod(_personPeriod);
            _person.AddSchedulePeriod(_schedulePeriod);

            _splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
            _target = new VirtualSchedulePeriod(_person, _dateOnly, _splitChecker);
        }

        [Test]
        public void CanCreateVirtualSchedulePeriod()
        {
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsValid);
            Assert.AreEqual(SchedulePeriodType.Week, _target.PeriodType);
            Assert.AreEqual(4, _target.Number);
            Assert.AreEqual(_person, _target.Person);
            Assert.IsNotNull(_target.ShiftCategoryLimitationCollection());
            Assert.AreEqual(0, _target.MustHavePreference);
        }

        [Test]
        public void CanGetNumberOfDaysOff()
        {
            Assert.AreEqual(8, _target.DaysOff());
            _schedulePeriod.DaysOff = 7;
            Assert.AreEqual(7, _target.DaysOff());
        }

		[Test]
		public void CanGetNumberOfDaysOffForChineseMonth()
		{
			_schedulePeriod.PeriodType = SchedulePeriodType.ChineseMonth;
			Assert.AreEqual(2, _target.DaysOff()); // 1/4/th of the original 8
		}

        [Test]
        public void CanGetNumberOfWorkdays()
        {
            Assert.AreEqual(20, _target.Workdays());
            _schedulePeriod.DaysOff = 7;
            Assert.AreEqual(21, _target.Workdays());
        }

        [Test]
        public void CanGetPeriodTarget()
        {
            Assert.AreEqual(TimeSpan.FromHours(8 * 20), _target.PeriodTarget());
            _schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6);
        }

        [Test]
        public void VerifyAverageWorkTimePerDay()
        {
            Assert.AreEqual(TimeSpan.FromHours(8), _target.AverageWorkTimePerDay);
			_schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(9);
            Assert.AreEqual(TimeSpan.FromHours(9), _target.AverageWorkTimePerDay);
        }

        [Test]
        public void ShouldHandleEmptyContractScheduleWeekWithPeriodWorkTimeOverridden()
        {
            _contractSchedule.ClearContractScheduleWeeks();
            _contractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
            _schedulePeriod.PeriodTime = TimeSpan.FromHours(168);

            Assert.AreEqual(TimeSpan.Zero, _target.AverageWorkTimePerDay);
        }

        [Test]
        public void CanGetEmptyListOfShiftCategoryLimitationsWhenNoSchedulePeriod()
        {
            var mocks = new MockRepository();
            _person = mocks.StrictMock<IPerson>();
            using (mocks.Record())
            {
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>()).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                _target = new VirtualSchedulePeriod(_person, _dateOnly, _splitChecker);
                Assert.IsTrue(_target.ShiftCategoryLimitationCollection().Count == 0);
            }
        }

        [Test]
        public void CanGetZeroTimeSpanForAverageWorkTimeWhenPersonPeriodIsNull()
        {
            _person.RemoveAllPersonPeriods();
            _target = new VirtualSchedulePeriod(_person, _dateOnly, _splitChecker);
            Assert.AreEqual(new TimeSpan(), _target.AverageWorkTimePerDay);
        }

        [Test]
        public void AnotherSchedulePeriodThatConflictsWithLastConvertsVirtualToDaily()
        {
            // the one before was supposed to end 2009-03-01
            var dateOnly = new DateOnly(2009, 2, 16);

            ISchedulePeriod schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 4);
            schedulePeriod.SetParent(_person);
            _person.AddSchedulePeriod(schedulePeriod);

            _target = new VirtualSchedulePeriod(_person, _dateOnly, _splitChecker);
            Assert.AreEqual(new DateOnly(2009, 2, 15), _target.DateOnlyPeriod.EndDate);
            Assert.AreEqual(14, _target.Number);
            Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
        }

        [Test]
        public void PersonTerminationConflictsWithLastConvertsVirtualToDaily()
        {
            // the one before was supposed to end 2009-03-01
            var mocks = new MockRepository();
            var dateOnly = new DateOnly(2009, 2, 16);
            var person = mocks.StrictMock<IPerson>();

            _schedulePeriod.SetParent(person);
            var permissions = new PermissionInformation(person);
            permissions.SetCulture(new CultureInfo("sv-SE"));
            using (mocks.Record())
            {
                Expect.Call(person.PermissionInformation).Return(permissions).Repeat.AtLeastOnce();
                Expect.Call(person.Period(_dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.TerminalDate).Return(dateOnly).Repeat.AtLeastOnce();
                Expect.Call(person.NextPeriod(_personPeriod)).Return(null);
                Expect.Call(person.Period(new DateOnly())).IgnoreArguments().Return(_personPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.PreviousPeriod(_personPeriod)).Return(null);
                Expect.Call(person.SchedulePeriodStartDate(_dateOnly)).Return(_dateOnly).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
                _target = new VirtualSchedulePeriod(person, _dateOnly, splitChecker);
                Assert.AreEqual(new DateOnly(2009, 2, 16), _target.DateOnlyPeriod.EndDate);
                Assert.AreEqual(15, _target.Number);
                Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
                Assert.AreEqual(4, _target.DaysOff());
                Assert.AreEqual(11, _target.Workdays());
                Assert.That(_target.BalanceIn, Is.EqualTo(TimeSpan.FromMinutes(165)));
                Assert.That(_target.BalanceOut, Is.EqualTo(TimeSpan.FromMinutes(198)));
                Assert.That(_target.Extra, Is.EqualTo(TimeSpan.FromMinutes(231)));
                Assert.That(_target.Seasonality, Is.EqualTo(new Percent(0.5)));
            }
        }

        [Test]
        public void FirstPersonPeriodIncurrentSchedulePeriodConvertsVirtualToDaily()
        {
            var mocks = new MockRepository();
            var dateOnly = new DateOnly(2009, 2, 16);
            IPersonPeriod firstPersonPeriod = new PersonPeriod(dateOnly, _personContract, new Team());

            var person = mocks.StrictMock<IPerson>();

            _schedulePeriod.SetParent(person);
            var permissions = new PermissionInformation(person);
            permissions.SetCulture(new CultureInfo("sv-SE"));

            using (mocks.Record())
            {
                Expect.Call(person.PermissionInformation).Return(permissions).Repeat.AtLeastOnce();
                Expect.Call(person.Period(new DateOnly(2009, 2, 20))).Return(firstPersonPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.Period(new DateOnly(2009, 02, 16))).Return(firstPersonPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
                Expect.Call(person.NextPeriod(firstPersonPeriod)).Return(null);
                Expect.Call(person.PreviousPeriod(firstPersonPeriod)).Return(null);
                Expect.Call(person.SchedulePeriodStartDate(dateOnly))
                      .Return(dateOnly).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
                _target = new VirtualSchedulePeriod(person, new DateOnly(2009, 2, 20), splitChecker);
                Assert.AreEqual(new DateOnly(2009, 2, 16), _target.DateOnlyPeriod.StartDate);
                Assert.AreEqual(new DateOnly(2009, 3, 1), _target.DateOnlyPeriod.EndDate);
                Assert.AreEqual(14, _target.Number);
                Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
                Assert.AreEqual(4, _target.DaysOff());
                Assert.AreEqual(10, _target.Workdays());
            }
        }

        [Test]
        public void NextPersonPeriodIncurrentSchedulePeriodConvertsVirtualToDaily()
        {
            var mocks = new MockRepository();
            var dateOnly = new DateOnly(2009, 2, 16);

            var person = mocks.StrictMock<IPerson>();
            var permissions = new PermissionInformation(person);
            permissions.SetCulture(new CultureInfo("sv-SE"));
            _schedulePeriod.SetParent(person);

            var contract = new Contract("con");
            var partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
            var personContract = new PersonContract(contract, partTime, _contractSchedule);
            var nextPersonPeriod = new PersonPeriod(dateOnly, personContract, new Team());

            using (mocks.Record())
            {
                Expect.Call(person.PermissionInformation).Return(permissions).Repeat.AtLeastOnce();
                Expect.Call(person.Period(_dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
                Expect.Call(person.NextPeriod(_personPeriod)).Return(nextPersonPeriod);

                Expect.Call(person.PreviousPeriod(_personPeriod)).Return(null);

                Expect.Call(person.SchedulePeriodStartDate(_dateOnly)).Return(_dateOnly).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
                _target = new VirtualSchedulePeriod(person, _dateOnly, splitChecker);
                Assert.AreEqual(new DateOnly(2009, 2, 15), _target.DateOnlyPeriod.EndDate);
                Assert.AreEqual(14, _target.Number);
                Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
                Assert.AreEqual(4, _target.DaysOff());
                Assert.AreEqual(10, _target.Workdays());
            }
        }

        [Test]
        public void ConfirmTerminalDateNewPersonPeriodNewSchedulePeriodAllConflictingIsHandledCorrect()
        {
            // the one before was supposed to end 2009-03-01
            var mocks = new MockRepository();
            var terminalDateOnly = new DateOnly(2009, 2, 16);
            var nextSchedulePeriodDateOnly = new DateOnly(2009, 2, 15);
            var nextPersonPeriodDateOnly = new DateOnly(2009, 2, 14);
            var person = mocks.StrictMock<IPerson>();

            _schedulePeriod.SetParent(person);

            ISchedulePeriod schedulePeriod = new SchedulePeriod(nextSchedulePeriodDateOnly, SchedulePeriodType.Week, 4);
            schedulePeriod.SetParent(person);
            _schedulePeriods.Add(schedulePeriod);

            var contract = new Contract("con");
            var partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
            var personContract = new PersonContract(contract, partTime, _contractSchedule);
            var nextPersonPeriod = new PersonPeriod(nextPersonPeriodDateOnly, personContract, new Team());


            var permissions = new PermissionInformation(person);
            permissions.SetCulture(new CultureInfo("sv-SE"));

            using (mocks.Record())
            {
                Expect.Call(person.PermissionInformation).Return(permissions).Repeat.AtLeastOnce();
                Expect.Call(person.Period(_dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.TerminalDate).Return(terminalDateOnly).Repeat.AtLeastOnce();
                Expect.Call(person.NextPeriod(_personPeriod)).Return(nextPersonPeriod);
                Expect.Call(person.PreviousPeriod(_personPeriod)).Return(null);
                Expect.Call(person.SchedulePeriodStartDate(_dateOnly)).Return(_dateOnly).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
                _target = new VirtualSchedulePeriod(person, _dateOnly, splitChecker);
                Assert.AreEqual(new DateOnly(2009, 2, 13), _target.DateOnlyPeriod.EndDate);
                Assert.AreEqual(12, _target.Number);
                Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
                Assert.AreEqual(2, _target.DaysOff());
                Assert.AreEqual(10, _target.Workdays());
            }
        }

        [Test]
        public void NotValidPeriodReturnZeroOnDaysOffAndWorkdays()
        {
            var mocks = new MockRepository();
            var person = mocks.StrictMock<IPerson>();

            _schedulePeriod.SetParent(person);

            ISchedulePeriod schedulePeriod = new SchedulePeriod(_dateOnly, SchedulePeriodType.Day, 9);
            schedulePeriod.SetParent(person);
            _schedulePeriods.Clear();
            _schedulePeriods.Add(schedulePeriod);
            var permissions = new PermissionInformation(person);
            permissions.SetCulture(new CultureInfo("sv-SE"));

            using (mocks.Record())
            {
                Expect.Call(person.PermissionInformation).Return(permissions);
                Expect.Call(person.Period(_dateOnly)).Return(null).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
                _target = new VirtualSchedulePeriod(person, _dateOnly, splitChecker);
                Assert.IsFalse(_target.IsValid);
                Assert.AreEqual(0, _target.DaysOff());
            }
        }

        [Test]
        public void VerifyRolledDaySchedule()
        {
            //22:e ska ge 20-28
            var mocks = new MockRepository();
            var requestedDateOnly = new DateOnly(2009, 2, 22);
            var person = mocks.StrictMock<IPerson>();
            var personPeriod = mocks.StrictMock<IPersonPeriod>();

            _schedulePeriod.SetParent(person);

            ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2009, 2, 2), SchedulePeriodType.Day, 9);
            schedulePeriod.SetParent(person);
            _schedulePeriods.Clear();
            _schedulePeriods.Add(schedulePeriod);
            var permissions = new PermissionInformation(person);
            permissions.SetCulture(new CultureInfo("sv-SE"));

        	using (mocks.Record())
        	{
        		Expect.Call(person.PermissionInformation).Return(permissions).Repeat.AtLeastOnce();
        		Expect.Call(person.Period(requestedDateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
        		Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
        		Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
        		Expect.Call(person.NextPeriod(_personPeriod)).Return(null);
        		Expect.Call(person.Period(new DateOnly(2009, 02, 20))).Return(personPeriod).Repeat.AtLeastOnce();
        		Expect.Call(personPeriod.PersonContract).Return(_personContract);
        		Expect.Call(person.SchedulePeriodStartDate(new DateOnly(2009, 02, 20))).Return(_dateOnly).Repeat.AtLeastOnce();

        		Expect.Call(person.PreviousPeriod(_personPeriod)).Return(null);
        	}
			using (mocks.Playback())
			{
				var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
				_target = new VirtualSchedulePeriod(person, requestedDateOnly, splitChecker);
				Assert.AreEqual(new DateOnly(2009, 2, 20), _target.DateOnlyPeriod.StartDate);
				Assert.AreEqual(new DateOnly(2009, 2, 28), _target.DateOnlyPeriod.EndDate);
				Assert.AreEqual(9, _target.Number);
				Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
				Assert.AreEqual(3, _target.DaysOff());
				Assert.AreEqual(6, _target.Workdays());
			}
        }

        [Test]
        public void SchedulePeriodStartsInsidePersonPeriodIsCorrect()
        {
            var mocks = new MockRepository();
            var dateOnly = new DateOnly(2009, 2, 20);
        	var dateOnly2 = new DateOnly(2009, 02, 15);

            var person = mocks.StrictMock<IPerson>();

            _schedulePeriod = new SchedulePeriod(new DateOnly(2009, 2, 15), SchedulePeriodType.Week, 4);
            _schedulePeriod.SetParent(person);
            _schedulePeriods.Clear();
            _schedulePeriods.Add(_schedulePeriod);
            var permissions = new PermissionInformation(person);
            permissions.SetCulture(new CultureInfo("sv-SE"));

            using (mocks.Record())
            {
                Expect.Call(person.PermissionInformation).Return(permissions).Repeat.AtLeastOnce();
                Expect.Call(person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.Period(new DateOnly(2009, 02, 15))).Return(_personPeriod).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
                Expect.Call(person.NextPeriod(_personPeriod)).Return(null);
                Expect.Call(person.SchedulePeriodStartDate(dateOnly2)).Return(dateOnly2).Repeat.AtLeastOnce();
                Expect.Call(person.PreviousPeriod(_personPeriod)).Return(null);
            }
            using (mocks.Playback())
            {
                var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
                _target = new VirtualSchedulePeriod(person, dateOnly, splitChecker);
                Assert.AreEqual(new DateOnly(2009, 2, 15), _target.DateOnlyPeriod.StartDate);
                Assert.AreEqual(4, _target.Number);
                Assert.AreEqual(SchedulePeriodType.Week, _target.PeriodType);
                Assert.AreEqual(8, _target.DaysOff());
                Assert.AreEqual(20, _target.Workdays());
            }
        }

        [Test]
        public void SchedulePeriodStartsInsidePersonPeriodAndTryingToGetOneEarlierReturnsInvalidPeriod()
        {
            var mocks = new MockRepository();
            var dateOnly = new DateOnly(2009, 2, 2);
            var person = mocks.StrictMock<IPerson>();

            _schedulePeriod = new SchedulePeriod(new DateOnly(2009, 2, 15), SchedulePeriodType.Week, 4);
            _schedulePeriod.SetParent(person);
            _schedulePeriods.Clear();
            _schedulePeriods.Add(_schedulePeriod);

            using (mocks.Record())
            {
                Expect.Call(person.PersonSchedulePeriodCollection).Return(_schedulePeriods).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
                _target = new VirtualSchedulePeriod(person, dateOnly, splitChecker);
                Assert.IsFalse((_target.IsValid));
            }
        }

        [Test]
        public void VerifyNotEqualsIfOtherType()
        {
            var testObj = new object();
            Assert.IsFalse(_target.Equals(testObj));
            Assert.AreNotEqual(_target.GetHashCode(), new object().GetHashCode());
        }

        [Test]
        public void VerifyEqualsSucceeds()
        {
            var testObj = new VirtualSchedulePeriod(_person, _dateOnly.AddDays(1), _splitChecker);
            Assert.IsTrue(_target.Equals(testObj));
            Assert.AreEqual(_target.GetHashCode(), testObj.GetHashCode());
        }

        [Test]
        public void VerifyEqualsFailsOnPerson()
        {
            var person = new Person();
            var splitChecker = new VirtualSchedulePeriodSplitChecker(person);
            var testObj = new VirtualSchedulePeriod(person, _dateOnly, splitChecker);
            Assert.IsFalse(_target.Equals(testObj));
            Assert.AreNotEqual(_target.GetHashCode(), testObj.GetHashCode());
        }

        [Test]
        public void VerifyEqualsFailsOnDate()
        {
            var testObj = new VirtualSchedulePeriod(_person, _dateOnly.AddDays(365), _splitChecker);
            Assert.IsFalse(_target.Equals(testObj));
            Assert.AreNotEqual(_target.GetHashCode(), testObj.GetHashCode());
        }

        [Test]
        public void ShouldReturnContractOnPersonPeriod()
        {
            Assert.That(_target.Contract, Is.Not.Null);
        }

        [Test]
        public void ShouldReturnContractScheduleOnPersonPeriod()
        {
            Assert.That(_target.ContractSchedule, Is.Not.Null);
        }

        [Test]
        public void ShouldReturnPartTimePercentageOnPersonPeriod()
        {
            Assert.That(_target.PartTimePercentage, Is.Not.Null);
        }

        [Test]
        public void CanGetStartDateTimePeriodForWeekSchedulePeriod()
        {
            var mocks = new MockRepository();
            var schedulePeriod = mocks.StrictMock<ISchedulePeriod>();
            var cultureInfo = CultureInfo.GetCultureInfo("sv-SE");
            using (mocks.Record())
            {
                Expect.Call(schedulePeriod.DateFrom).Return(new DateOnly(2010, 9, 27));
                Expect.Call(schedulePeriod.PeriodType).Return(SchedulePeriodType.Week);
                Expect.Call(schedulePeriod.Number).Return(4);
            }
            using (mocks.Playback())
            {

                var ret = VirtualSchedulePeriod.GetOriginalStartPeriodForType(schedulePeriod, cultureInfo);
                Assert.That(ret.StartDate, Is.EqualTo(new DateOnly(2010, 9, 27)));
                Assert.That(ret.EndDate, Is.EqualTo(new DateOnly(2010, 10, 24)));
            }
        }

        [Test]
        public void CanGetStartDateTimePeriodForMonthSchedulePeriod()
        {
            var mocks = new MockRepository();
            var schedulePeriod = mocks.StrictMock<ISchedulePeriod>();
            var cultureInfo = CultureInfo.GetCultureInfo("sv-SE");

            using (mocks.Record())
            {
                Expect.Call(schedulePeriod.DateFrom).Return(new DateOnly(2010, 9, 27));
                Expect.Call(schedulePeriod.PeriodType).Return(SchedulePeriodType.Month);
                Expect.Call(schedulePeriod.Number).Return(1);
            }
            using (mocks.Playback())
            {

                var ret = VirtualSchedulePeriod.GetOriginalStartPeriodForType(schedulePeriod, cultureInfo);
                Assert.That(ret.StartDate, Is.EqualTo(new DateOnly(2010, 9, 27)));
                Assert.That(ret.EndDate, Is.EqualTo(new DateOnly(2010, 10, 26)));
            }
        }

        [Test]
        public void CanGetStartDateTimePeriodForDaySchedulePeriod()
        {
            var mocks = new MockRepository();
            var schedulePeriod = mocks.StrictMock<ISchedulePeriod>();
            var cultureInfo = CultureInfo.GetCultureInfo("sv-SE");

            using (mocks.Record())
            {
                Expect.Call(schedulePeriod.DateFrom).Return(new DateOnly(2010, 9, 27));
                Expect.Call(schedulePeriod.PeriodType).Return(SchedulePeriodType.Day);
                Expect.Call(schedulePeriod.Number).Return(30);
            }
            using (mocks.Playback())
            {
                var ret = VirtualSchedulePeriod.GetOriginalStartPeriodForType(schedulePeriod, cultureInfo);
                Assert.That(ret.StartDate, Is.EqualTo(new DateOnly(2010, 9, 27)));
                Assert.That(ret.EndDate, Is.EqualTo(new DateOnly(2010, 10, 26)));
            }
        }

        [Test]
        public void NoIntersectionWithOriginalShouldReturnZeroPercent()
        {
            var originalPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 31);
            var ret = _target.GetPercentageWorkdaysOfOriginalPeriod(originalPeriod, null);
            Assert.That(ret, Is.EqualTo(new Percent(0)));
        }

        [Test]
        public void ShouldReturnTrueIfPeriodIsFirst()
        {
            var result = _target.IsOriginalPeriod();
            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldReturnFalseIfPeriodIsRolled()
        {
            _target = new VirtualSchedulePeriod(_person, _dateOnly.AddDays(50), _splitChecker);

            var result = _target.IsOriginalPeriod();
            Assert.That(result, Is.False);
        }
    }
}
