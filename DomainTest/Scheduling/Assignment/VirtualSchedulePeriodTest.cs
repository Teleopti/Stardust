using System;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;


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
        private IPersonContract _personContract;
        private IContractSchedule _contractSchedule;
        private ITeam _team;
        private ISite _site;
        private IVirtualSchedulePeriodSplitChecker _splitChecker;

        [SetUp]
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

            _team = new Team();
            _site = new Site("Site");
            _site.AddTeam(_team);

            _personPeriod = new PersonPeriod(_dateOnly, _personContract, _team);
            IRuleSetBag ruleSetBag = new RuleSetBag();
            _personPeriod.RuleSetBag = ruleSetBag;
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
		    _person.RemoveAllSchedulePeriods();

		    _target = new VirtualSchedulePeriod(_person, _dateOnly, _splitChecker);
		    Assert.IsTrue(_target.ShiftCategoryLimitationCollection().Count == 0);
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
		    _person.TerminatePerson(new DateOnly(2009, 2, 16),new PersonAccountUpdaterDummy());
		    _person.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, _dateOnly, splitChecker);
		    Assert.AreEqual(new DateOnly(2009, 2, 16), _target.DateOnlyPeriod.EndDate);
		    Assert.AreEqual(15, _target.Number);
		    Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
		    Assert.AreEqual(4, _target.DaysOff());
		    Assert.AreEqual(11, _target.Workdays());
		    Assert.That(_target.BalanceIn, Is.EqualTo(TimeSpan.FromMinutes(300)));
		    Assert.That(_target.BalanceOut, Is.EqualTo(TimeSpan.FromMinutes(360)));
		    Assert.That(_target.Extra, Is.EqualTo(TimeSpan.FromMinutes(420)));
		    Assert.That(_target.Seasonality, Is.EqualTo(new Percent(0.5)));
	    }

	    [Test]
	    public void FirstPersonPeriodIncurrentSchedulePeriodConvertsVirtualToDaily()
	    {
		    var dateOnly = new DateOnly(2009, 2, 16);
		    IPersonPeriod firstPersonPeriod = new PersonPeriod(dateOnly, _personContract, new Team());

		    _person.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
		    _person.RemoveAllPersonPeriods();
		    _person.AddPersonPeriod(firstPersonPeriod);

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, new DateOnly(2009, 2, 20), splitChecker);
		    Assert.AreEqual(new DateOnly(2009, 2, 16), _target.DateOnlyPeriod.StartDate);
		    Assert.AreEqual(new DateOnly(2009, 3, 1), _target.DateOnlyPeriod.EndDate);
		    Assert.AreEqual(14, _target.Number);
		    Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
		    Assert.AreEqual(4, _target.DaysOff());
		    Assert.AreEqual(10, _target.Workdays());
	    }

	    [Test]
	    public void NextPersonPeriodIncurrentSchedulePeriodConvertsVirtualToDaily()
	    {
		    var dateOnly = new DateOnly(2009, 2, 16);

		    _person.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));

		    var contract = new Contract("con");
		    var partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
		    var personContract = new PersonContract(contract, partTime, _contractSchedule);
		    var nextPersonPeriod = new PersonPeriod(dateOnly, personContract, new Team());

		    _person.AddPersonPeriod(nextPersonPeriod);

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, _dateOnly, splitChecker);
		    Assert.AreEqual(new DateOnly(2009, 2, 15), _target.DateOnlyPeriod.EndDate);
		    Assert.AreEqual(14, _target.Number);
		    Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
		    Assert.AreEqual(4, _target.DaysOff());
		    Assert.AreEqual(10, _target.Workdays());
	    }

	    [Test]
	    public void ConfirmTerminalDateNewPersonPeriodNewSchedulePeriodAllConflictingIsHandledCorrect()
	    {
		    // the one before was supposed to end 2009-03-01
		    var terminalDateOnly = new DateOnly(2009, 2, 16);
		    var nextSchedulePeriodDateOnly = new DateOnly(2009, 2, 15);
		    var nextPersonPeriodDateOnly = new DateOnly(2009, 2, 14);

		    ISchedulePeriod schedulePeriod = new SchedulePeriod(nextSchedulePeriodDateOnly, SchedulePeriodType.Week, 4);
		    _person.AddSchedulePeriod(schedulePeriod);

		    var contract = new Contract("con");
		    var partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
		    var personContract = new PersonContract(contract, partTime, _contractSchedule);
		    var nextPersonPeriod = new PersonPeriod(nextPersonPeriodDateOnly, personContract, new Team());
			_person.AddPersonPeriod(nextPersonPeriod);

		    _person.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
			_person.TerminatePerson(terminalDateOnly,new PersonAccountUpdaterDummy());

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, _dateOnly, splitChecker);
		    Assert.AreEqual(new DateOnly(2009, 2, 13), _target.DateOnlyPeriod.EndDate);
		    Assert.AreEqual(12, _target.Number);
		    Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
		    Assert.AreEqual(2, _target.DaysOff());
		    Assert.AreEqual(10, _target.Workdays());
	    }

	    [Test]
	    public void NotValidPeriodReturnZeroOnDaysOffAndWorkdays()
	    {
		    ISchedulePeriod schedulePeriod = new SchedulePeriod(_dateOnly, SchedulePeriodType.Day, 9);
		    _person.RemoveAllSchedulePeriods();
		    _person.RemoveAllPersonPeriods();
		    _person.AddSchedulePeriod(schedulePeriod);

		    _person.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, _dateOnly, splitChecker);
		    Assert.IsFalse(_target.IsValid);
		    Assert.AreEqual(0, _target.DaysOff());
	    }

	    [Test]
	    public void VerifyRolledDaySchedule()
	    {
		    //22:e ska ge 20-28
		    var requestedDateOnly = new DateOnly(2009, 2, 22);

		    ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2009, 2, 2), SchedulePeriodType.Day, 9);
		    schedulePeriod.SetParent(_person);
		    _person.RemoveAllSchedulePeriods();
		    _person.AddSchedulePeriod(schedulePeriod);

		    _person.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, requestedDateOnly, splitChecker);
		    Assert.AreEqual(new DateOnly(2009, 2, 20), _target.DateOnlyPeriod.StartDate);
		    Assert.AreEqual(new DateOnly(2009, 2, 28), _target.DateOnlyPeriod.EndDate);
		    Assert.AreEqual(9, _target.Number);
		    Assert.AreEqual(SchedulePeriodType.Day, _target.PeriodType);
		    Assert.AreEqual(3, _target.DaysOff());
		    Assert.AreEqual(6, _target.Workdays());
	    }

	    [Test]
	    public void SchedulePeriodStartsInsidePersonPeriodIsCorrect()
	    {
		    var dateOnly = new DateOnly(2009, 2, 20);
		    var dateOnly2 = new DateOnly(2009, 02, 15);

		    _person.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));

		    _schedulePeriod = new SchedulePeriod(dateOnly2, SchedulePeriodType.Week, 4);
		    _person.RemoveAllSchedulePeriods();
		    _person.AddSchedulePeriod(_schedulePeriod);

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, dateOnly, splitChecker);
		    Assert.AreEqual(new DateOnly(2009, 2, 15), _target.DateOnlyPeriod.StartDate);
		    Assert.AreEqual(4, _target.Number);
		    Assert.AreEqual(SchedulePeriodType.Week, _target.PeriodType);
		    Assert.AreEqual(8, _target.DaysOff());
		    Assert.AreEqual(20, _target.Workdays());
	    }

	    [Test]
	    public void SchedulePeriodStartsInsidePersonPeriodAndTryingToGetOneEarlierReturnsInvalidPeriod()
	    {
		    var dateOnly = new DateOnly(2009, 2, 2);

		    _schedulePeriod = new SchedulePeriod(new DateOnly(2009, 2, 15), SchedulePeriodType.Week, 4);
		    _person.RemoveAllSchedulePeriods();
		    _person.AddSchedulePeriod(_schedulePeriod);

		    var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
		    _target = new VirtualSchedulePeriod(_person, dateOnly, splitChecker);
		    Assert.IsFalse((_target.IsValid));
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
		    var schedulePeriod = new SchedulePeriod(new DateOnly(2010, 9, 27), SchedulePeriodType.Week, 4);
		    var cultureInfo = CultureInfo.GetCultureInfo("sv-SE");

		    var ret = VirtualSchedulePeriod.GetOriginalStartPeriodForType(schedulePeriod, cultureInfo);
		    Assert.That(ret.StartDate, Is.EqualTo(new DateOnly(2010, 9, 27)));
		    Assert.That(ret.EndDate, Is.EqualTo(new DateOnly(2010, 10, 24)));
	    }

	    [Test]
	    public void CanGetStartDateTimePeriodForMonthSchedulePeriod()
	    {
		    var schedulePeriod = new SchedulePeriod(new DateOnly(2010, 9, 27), SchedulePeriodType.Month, 1);
		    var cultureInfo = CultureInfo.GetCultureInfo("sv-SE");

		    var ret = VirtualSchedulePeriod.GetOriginalStartPeriodForType(schedulePeriod, cultureInfo);
		    Assert.That(ret.StartDate, Is.EqualTo(new DateOnly(2010, 9, 27)));
		    Assert.That(ret.EndDate, Is.EqualTo(new DateOnly(2010, 10, 26)));
	    }

	    [Test]
	    public void CanGetStartDateTimePeriodForDaySchedulePeriod()
	    {
		    var schedulePeriod = new SchedulePeriod(new DateOnly(2010, 9, 27), SchedulePeriodType.Day, 30);
		    var cultureInfo = CultureInfo.GetCultureInfo("sv-SE");

		    var ret = VirtualSchedulePeriod.GetOriginalStartPeriodForType(schedulePeriod, cultureInfo);
		    Assert.That(ret.StartDate, Is.EqualTo(new DateOnly(2010, 9, 27)));
		    Assert.That(ret.EndDate, Is.EqualTo(new DateOnly(2010, 10, 26)));
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

		[Test]
		public void CanGetZeroTimeSpanForBalanceInWhenSchedulePeriodIsNull()
		{
			ISchedulePeriod schedulePeriod = null;
			_target = new VirtualSchedulePeriod(_person, _dateOnly.AddDays(50), _personPeriod, schedulePeriod, _splitChecker);
			Assert.AreEqual(TimeSpan.FromMinutes(0), _target.BalanceIn);
		}

		[Test]
		public void CanGetZeroTimeSpanForBalanceOutWhenSchedulePeriodIsNull()
		{
			ISchedulePeriod schedulePeriod = null;
			_target = new VirtualSchedulePeriod(_person, _dateOnly.AddDays(50), _personPeriod, schedulePeriod, _splitChecker);
			Assert.AreEqual(TimeSpan.FromMinutes(0), _target.BalanceOut);
		}

		[Test]
		public void CanGetZeroTimeSpanForExtraWhenSchedulePeriodIsNull()
		{
			ISchedulePeriod schedulePeriod = null;
			_target = new VirtualSchedulePeriod(_person, _dateOnly.AddDays(50), _personPeriod, schedulePeriod, _splitChecker);
			Assert.AreEqual(TimeSpan.FromMinutes(0), _target.Extra);
		}
	}
}
