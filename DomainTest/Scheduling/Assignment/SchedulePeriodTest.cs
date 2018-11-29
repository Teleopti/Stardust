using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
	
    public class SchedulePeriodTest
    {
        private DateOnly _from;
        private SchedulePeriodType _type;
        private int _number;
        private SchedulePeriod _periodDay;
        private SchedulePeriod _periodWeek;
        private SchedulePeriod _periodMonth;
		private SchedulePeriod _periodChineseMonth;
        private TimeSpan _avgWorkTimePerDay;
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
		private IPerson _person4;
        private IPerson _normalPerson;
        private IPersonContract _personContract;
        private IContract _contract1;
        private int _mustHavePreference;
        private MockRepository _mocks;
        private IPersonAccountUpdater _personAccountUpdater;

        private void setup()
        {
            _person1 = PersonFactory.CreatePerson();
            _person1.PermissionInformation.SetCulture(new CultureInfo("en-US"));
            _person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))); //GMT-3
            ITeam simpleTeam = TeamFactory.CreateSimpleTeam();
            IContract contract = ContractFactory.CreateContract("MyContract");
            IPartTimePercentage partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
            IContractSchedule contractSchedule = ContractScheduleFactory.CreateContractSchedule("Mon-Sat");
            IContractScheduleWeek week1 = new ContractScheduleWeek();
            week1.Add(DayOfWeek.Monday,true);
            week1.Add(DayOfWeek.Tuesday, true);
            week1.Add(DayOfWeek.Wednesday, true);
            week1.Add(DayOfWeek.Thursday, true);
            week1.Add(DayOfWeek.Friday, true);
            contractSchedule.AddContractScheduleWeek(week1);
            _personContract = new PersonContract(contract,partTime,contractSchedule);
            _normalPerson = PersonFactory.CreatePerson();
            _normalPerson.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
            _normalPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()); 
            _normalPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 1, 1), _personContract, simpleTeam));
            
			_person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 1, 3), _personContract, simpleTeam));
            
			_person2 = PersonFactory.CreatePerson();
            _person2.PermissionInformation.SetCulture(new CultureInfo("en-US"));
            _person2.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))); //GMT-3
            _person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2007, 12, 30), _personContract, simpleTeam));

            _person3 = PersonFactory.CreatePerson();
            _person3.PermissionInformation.SetCulture(new CultureInfo("en-US"));
            _person3.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))); //GMT-3
            _person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 1, 1), _personContract, simpleTeam));

			_person4 = PersonFactory.CreatePerson();
			_person4.PermissionInformation.SetCulture(new CultureInfo("en-US"));
			_person4.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))); //GMT-3
			_person4.AddPersonPeriod(new PersonPeriod(new DateOnly(2008, 1, 1), _personContract, simpleTeam));


            _from = new DateOnly(2008, 1, 3); //Thursday, sunday before is 12-30
            _type = SchedulePeriodType.Day;
            _number = 10;
            _avgWorkTimePerDay = TimeSpan.FromHours(8d);
            
			_periodDay = new SchedulePeriod(_from, _type, _number);
            _person1.AddSchedulePeriod(_periodDay);
            
			_periodWeek = new SchedulePeriod(_from, SchedulePeriodType.Week, 3);
            _person2.AddSchedulePeriod(_periodWeek);

            _periodMonth = new SchedulePeriod(_from, SchedulePeriodType.Month, 1);
            _person3.AddSchedulePeriod(_periodMonth);

			_periodChineseMonth = new SchedulePeriod(_from, SchedulePeriodType.ChineseMonth, 1);
			_person4.AddSchedulePeriod(_periodChineseMonth);

            _mustHavePreference = 3;

            _mocks = new MockRepository();
            _personAccountUpdater = _mocks.StrictMock<IPersonAccountUpdater>();
        }

		[Test]
		public void ShouldReturnZeroWorkTimePerDayWhenPeriodTimeOverrideAndSchedulePeriodStartsBeforePersonPeriod()
		{
			setup();
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetCulture(new CultureInfo("en-US"));
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))); //GMT-3
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 1, 1), _personContract, TeamFactory.CreateSimpleTeam()));
			var periodMonth = new SchedulePeriod(new DateOnly(2015, 3, 1), SchedulePeriodType.Month, 1)
			{
				PeriodTime = TimeSpan.Zero
			};
			person.AddSchedulePeriod(periodMonth);

			Assert.AreEqual(TimeSpan.Zero, periodMonth.AverageWorkTimePerDay);
		}

        [Test]
        public void CanCreateAndReadProperties()
        {
			setup();
			Assert.AreEqual(_from, _periodDay.DateFrom);
            Assert.AreEqual(_type, _periodDay.PeriodType);
            Assert.AreEqual(_number, _periodDay.Number);
            Assert.AreEqual(_avgWorkTimePerDay, _periodDay.AverageWorkTimePerDay);
            Assert.AreEqual(3, _periodDay.GetDaysOff(_from));

            Assert.AreEqual(_from, _periodMonth.DateFrom);
            Assert.AreEqual(_from, _periodWeek.DateFrom);
            Assert.AreEqual(0, _periodMonth.ShiftCategoryLimitationCollection().Count);
        }

        [Test]
        public void CanReturnCorrectWeekPeriod()
        {
			setup();
			var startPeriod1 = new DateOnly(2008, 12, 29);//Monday
            var startPeriod2 = new DateOnly(2009, 1, 5);
            var startPeriod3 = new DateOnly(2009, 1, 26);

            var weekPeriod1 = new SchedulePeriod(startPeriod1, SchedulePeriodType.Week, 2); 
            _normalPerson.AddSchedulePeriod(weekPeriod1);
            var weekPeriod2 = new SchedulePeriod(startPeriod2, SchedulePeriodType.Week, 1);
            _normalPerson.AddSchedulePeriod(weekPeriod2);
            var weekPeriod3 = new SchedulePeriod(startPeriod3, SchedulePeriodType.Week, 1); 
            _normalPerson.AddSchedulePeriod(weekPeriod3);
            var expectedDate = new DateOnly(2009,1,12);
            var requestdDateTime = new DateOnly(2009, 1, 15);
            var periodStart =
                _normalPerson.SchedulePeriod(requestdDateTime).GetSchedulePeriod(requestdDateTime).Value.StartDate;
            Assert.AreEqual(weekPeriod1, _normalPerson.SchedulePeriod(new DateOnly(2008, 12, 31)));
            Assert.AreEqual(weekPeriod2, _normalPerson.SchedulePeriod(new DateOnly(2009, 1, 5)));
            Assert.AreEqual(weekPeriod2, _normalPerson.SchedulePeriod(new DateOnly(2009, 1, 13)));
            Assert.AreEqual(expectedDate, periodStart);
            Assert.AreEqual(weekPeriod3, _normalPerson.SchedulePeriod(new DateOnly(2009, 3, 6)));
        }

        [Test]
        public void CanGetSchedulePeriodWeek()
        {
			setup();
			DateOnly startPeriod1 = new DateOnly(2008, 12, 30);//Tuesday
            ISchedulePeriod weekPeriod1 = new SchedulePeriod(startPeriod1, SchedulePeriodType.Week, 2);
            _normalPerson.AddSchedulePeriod(weekPeriod1);

            DateOnly requested = new DateOnly(2008, 12, 31);
            DateOnly expectedLocalStart = new DateOnly(2008, 12, 30);
            DateOnly expectedLocalEnd = new DateOnly(2009, 1, 12);
            //Convert expected local from person timezone
            DateOnlyPeriod expectedPeriod = new DateOnlyPeriod(expectedLocalStart, expectedLocalEnd);

            Assert.AreEqual(expectedPeriod, weekPeriod1.GetSchedulePeriod(requested));
        }

        [Test]
        public void CanGetSchedulePeriodDay()
        {
			setup();
			DateOnly requested = new DateOnly(2008, 3, 15);
            DateOnly expectedLocalStart = new DateOnly(2008, 3, 13);
            DateOnly expectedLocalEnd = new DateOnly(2008, 3, 22);
            DateOnlyPeriod expectedPeriod = new DateOnlyPeriod(expectedLocalStart, expectedLocalEnd);

            Assert.AreEqual(expectedPeriod, _periodDay.GetSchedulePeriod(requested));
        }

        [Test]
        public void CanGetSchedulePeriodMonth()
        {
			setup();
			DateOnly requested = new DateOnly(2008, 3, 15);
            DateOnly expectedLocalStart = new DateOnly(2008, 3, 3);
            DateOnly expectedLocalEnd = new DateOnly(2008, 4, 2);
            //Convert expected local from person timezone
            DateOnlyPeriod expectedPeriod = new DateOnlyPeriod(expectedLocalStart, expectedLocalEnd);

            Assert.AreEqual(expectedPeriod, _periodMonth.GetSchedulePeriod(requested));
        }

        [Test]
        public void CanGetSchedulePeriodMonthForTwoMonths()
        {
            DateOnly requested = new DateOnly(2008, 3, 15);
            DateOnly expectedLocalStart = new DateOnly(2008, 3, 3);
            DateOnly expectedLocalEnd = new DateOnly(2008, 5, 2);
            //Convert expected local from person timezone
            DateOnlyPeriod expectedPeriod = new DateOnlyPeriod(expectedLocalStart, expectedLocalEnd);

            _periodMonth.Number = 2;
            Assert.AreEqual(expectedPeriod, _periodMonth.GetSchedulePeriod(requested));
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        //[Test]
        //public void VerifyPeriodTarget()
        //{
        //    DateOnly schedulePeriodStart = new DateOnly(2009, 1, 5); // Mon
        //    SchedulePeriodType periodType = SchedulePeriodType.Day;
        //    int periodNumber = 10;
        //    TimeSpan avgWorkTimePerDay = TimeSpan.FromHours(8d);

        //    SchedulePeriod period = new SchedulePeriod(schedulePeriodStart, periodType, periodNumber);
        //    period.AverageWorkTimePerDay = avgWorkTimePerDay;

        //    // test 1: no person is given
        //    TimeSpan targetTime = period.PeriodTarget(schedulePeriodStart);
        //    Assert.AreEqual(0, targetTime.TotalHours);

        //    // test 2: person is given, no day off in contract
        //    DateOnly periodPeriodStart = new DateOnly(schedulePeriodStart.Date.Subtract(new TimeSpan(7, 0, 0, 0)));
        //    IPerson person = PersonFactory.CreatePersonWithPersonPeriod(periodPeriodStart, new List<ISkill>());
        //    person.AddSchedulePeriod(period);

        //    // 10 workdays, no day offs
        //    targetTime = period.PeriodTarget(schedulePeriodStart);
        //    Assert.AreEqual(10 * avgWorkTimePerDay.TotalHours, targetTime.TotalHours);

        //    // add days off in contract
        //    IContractSchedule contractSchedule = ContractScheduleFactory.CreateContractSchedule("Mon-Sat");
        //    IContractScheduleWeek week1 = new ContractScheduleWeek();
        //    week1.Add(DayOfWeek.Monday, true);
        //    week1.Add(DayOfWeek.Tuesday, true);
        //    week1.Add(DayOfWeek.Wednesday, true);
        //    week1.Add(DayOfWeek.Thursday, true);
        //    week1.Add(DayOfWeek.Friday, true);
        //    contractSchedule.AddContractScheduleWeek(week1);
        //    IContract contract = ContractFactory.CreateContract("MyContract");
        //    IPartTimePercentage partTime = PartTimePercentageFactory.CreatePartTimePercentage("Full time");
        //    PersonContract personContract = new PersonContract(contract, partTime, contractSchedule);
        //    person.PersonPeriodCollection[0].PersonContract = personContract;

        //    // 8 workdays, 2 offs
        //    targetTime = period.PeriodTarget(schedulePeriodStart);
        //    Assert.AreEqual(8 * avgWorkTimePerDay.TotalHours, targetTime.TotalHours);
        //}
 
        //[Test]
        //public void VerifyTargetTimeWithDifferentPeriodType()
        //{
        //    TimeSpan targetTime = _periodDay.PeriodTarget(new DateOnly(2008, 3, 5));
        //    Assert.AreEqual(TimeSpan.FromHours(8*8), targetTime);
        //    targetTime = _periodWeek.PeriodTarget(new DateOnly(2008, 3, 5));
        //    Assert.AreEqual(TimeSpan.FromHours(((3*5))*8), targetTime);
        //    targetTime = _periodMonth.PeriodTarget(new DateOnly(2008, 3, 5));
        //    Assert.AreEqual(TimeSpan.FromHours(23*8), targetTime);
        //}

        [Test]
        public void VerifyEmptyConstructor()
        {
			setup();
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_periodDay.GetType(), true));
        }

        [Test]
        public void VerifyNullOnLowerFromDate()
        {
			setup();
			DateOnly date = new DateOnly(2004, 1, 5);
            Assert.IsNull(_periodDay.GetSchedulePeriod(date));      
        }

        //[Test]
        //public void VerifyZeroTargetTimeOnLowerFromDate()
        //{
        //    DateOnly date = new DateOnly(2004, 1, 5);
        //    Assert.AreEqual(TimeSpan.FromMinutes(0), _periodDay.PeriodTarget(date));
        //}

        [Test]
        public void VerifyAdjustForTerminalDateInSchedulePeriod()
        {
			setup();
			DateOnly terminalDate = new DateOnly(2008, 3, 20);
            _person2.TerminatePerson(terminalDate,_personAccountUpdater);

            DateOnly requested = new DateOnly(2008, 3, 15);
            DateOnly expectedLocalStart = new DateOnly(2008, 3, 6);
            DateOnly expectedLocalEnd = terminalDate;
            DateOnlyPeriod expectedPeriod = new DateOnlyPeriod(expectedLocalStart, expectedLocalEnd);

            Assert.AreEqual(expectedPeriod, _periodWeek.GetSchedulePeriod(requested));
        }

        [Test]
        public void VerifyAdjustForTerminalDateBeforeSchedulePeriod()
        {
			setup();
			DateOnly terminalDate = new DateOnly(2008, 1, 20);
            _person2.TerminatePerson(terminalDate, _personAccountUpdater);
            DateOnly requested = new DateOnly(2008, 3, 15);
            Assert.IsFalse(_periodWeek.GetSchedulePeriod(requested) != null);    
        }

        [Test]
        public void VerifyAdjustForTerminalDateAfterSchedulePeriod()
        {
			setup();
			DateOnly terminalDate = new DateOnly(2008, 5, 20);
            _person2.TerminatePerson(terminalDate, _personAccountUpdater);

            DateOnly requested = new DateOnly(2008, 3, 15);
            DateOnly expectedStart = new DateOnly(2008, 3, 6);
            DateOnly expectedEnd = new DateOnly(2008, 3, 26);
            DateOnlyPeriod expectedPeriod =
                new DateOnlyPeriod(expectedStart, expectedEnd);

            Assert.AreEqual(expectedPeriod.StartDate, _periodWeek.GetSchedulePeriod(requested).Value.StartDate);
            Assert.AreEqual(expectedPeriod.EndDate, _periodWeek.GetSchedulePeriod(requested).Value.EndDate);
        }

        /// <summary>
        /// Verifies the can get contracts.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-23
        /// </remarks>
        [Test]
        public void VerifyCanGetContract()
        {
			setup();
			IPersonPeriod personPeriod;
            _person3.DeletePersonPeriod(_person3.PersonPeriodCollection.First());
            personPeriod = _periodMonth.GetPersonPeriod();
            Assert.IsNull(personPeriod);

            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 12, 30));
            _person3.AddPersonPeriod(personPeriod1);
            
            personPeriod = _periodMonth.GetPersonPeriod();
            Assert.AreEqual(personPeriod1,personPeriod);
        }

        [Test]
        public void VerifySeemsToGetRightPeriod()
        {
			setup();
			_contract1 = new Contract("4Hour");
            _contract1.WorkTime = new WorkTime(new TimeSpan(4,0,0));
            
            IPersonContract personContract1 = new PersonContract(_contract1, 
                                                                 new PartTimePercentage("Percent"),
                                                                 new ContractSchedule("Contract"));

            IPersonPeriod period1 = new PersonPeriod(new DateOnly(2009, 2, 2), personContract1, new Team());

            IContract contract2 = new Contract("8Hour");
            contract2.WorkTime = new WorkTime(new TimeSpan(8, 0, 0));

            IPersonContract personContract2 = new PersonContract(contract2,
                                                                 new PartTimePercentage("Percent2"),
                                                                 new ContractSchedule("Contrac2"));
            DateOnly dateOnly09 = new DateOnly(2009, 2, 9);

            IPersonPeriod period2 = new PersonPeriod(dateOnly09, personContract2, new Team());

            IPerson person = PersonFactory.CreatePerson();
 
            person.AddPersonPeriod(period1);
            person.AddPersonPeriod(period2);

            ISchedulePeriod schedulePeriod = new SchedulePeriod(dateOnly09, SchedulePeriodType.Day, 1);

            person.AddSchedulePeriod(schedulePeriod);

            IList<IPersonPeriod> periods = person.PersonPeriods(new DateOnlyPeriod(dateOnly09, dateOnly09.AddDays(10)));

            Assert.AreEqual(TimeSpan.FromHours(8), periods[0].PersonContract.Contract.WorkTime.AvgWorkTimePerDay);
        }



        /// <summary>
        /// Verifies the can get average work time from contract.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-23
        /// </remarks>
        [Test]
        public void VerifyCanGetAverageWorkTimeFromContract()
        {
			setup();
			IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2007, 12, 30));
            _person3.AddPersonPeriod(personPeriod1);

            _periodMonth.ResetAverageWorkTimePerDay();
            Assert.AreEqual(personPeriod1.PersonContract.AverageWorkTimePerDay, _periodMonth.AverageWorkTimePerDay);
            //Assert.AreEqual(personPeriod1.PersonContract.PartTimePercentage.);
        }

        /// <summary>
        /// Verifies the can reset average work time per day.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-23
        /// </remarks>
        [Test]
        public void VerifyCanResetAverageWorkTimePerDay()
        {
			setup();
			TimeSpan originalValue = _periodMonth.AverageWorkTimePerDay; //From contract time = 8 hours
			_periodMonth.AverageWorkTimePerDayOverride = TimeSpan.FromHours(5d);
            Assert.AreEqual(TimeSpan.FromHours(5d),_periodMonth.AverageWorkTimePerDay);
			Assert.IsTrue(_periodMonth.IsAverageWorkTimePerDayOverride);
            _periodMonth.ResetAverageWorkTimePerDay();
            Assert.AreEqual(originalValue,_periodMonth.AverageWorkTimePerDay);
			Assert.IsFalse(_periodMonth.IsAverageWorkTimePerDayOverride);
			Assert.AreEqual(TimeSpan.MinValue, _periodMonth.AverageWorkTimePerDayOverride);
        }

		[Test]
		public void VerifyAverageWorkTimePerDayWhenPeriodTimeIsOverridden()
		{
			setup();
			Assert.AreEqual(TimeSpan.FromHours(8d), _periodMonth.AverageWorkTimePerDay); // original value
			const int originalPeriodTime = 176;
			const int workdays = 22;
			_periodMonth.PeriodTime = new TimeSpan(originalPeriodTime + workdays, 0, 0); 
			Assert.IsTrue(_periodMonth.IsPeriodTimeOverride);
			Assert.AreEqual(TimeSpan.FromHours(9d), _periodMonth.AverageWorkTimePerDay);
		}

        /// <summary>
        /// Verifies the can reset days off.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-10
        /// </remarks>
        [Test]
        public void VerifyCanResetDaysOff()
        {
			setup();
			int originalValue = _periodMonth.GetDaysOff(_from); //From contract = 8
            _periodMonth.SetDaysOff(5);
			Assert.IsTrue(_periodMonth.DaysOff.HasValue);
            Assert.AreEqual(5, _periodMonth.GetDaysOff(_from));
            Assert.AreEqual(5, _periodMonth.GetDaysOff(new DateOnly(_from.Date.AddYears(1))));
            _periodMonth.ResetDaysOff();
            Assert.AreEqual(originalValue, _periodMonth.GetDaysOff(_from));
			Assert.IsFalse(_periodMonth.DaysOff.HasValue);
        }

		[Test]
		public void VerifyCanResetPeriodTime()
		{
			setup();
			_periodMonth.PeriodTime = new TimeSpan(1000);
			Assert.IsTrue(_periodMonth.PeriodTime.HasValue);
			_periodMonth.ResetPeriodTime();
			Assert.IsFalse(_periodMonth.PeriodTime.HasValue);
		}

        /// <summary>
        /// Verifies the number of days off cannot be less than one.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-10
        /// </remarks>
        [Test]
        public void VerifyNumberOfDaysOffCannotBeLessThanOne()
        {
			setup();
			Assert.Throws<ArgumentOutOfRangeException>(() => _periodMonth.SetDaysOff(-1));
        }

        [Test]
        public void VerifyNumberOfDaysOffCannotBeGreaterThan999()
        {
			setup();
			Assert.Throws<ArgumentOutOfRangeException>(() => _periodMonth.SetDaysOff(1000));
        }

        [Test]
        public void VerifyNumberOfDaysOffOk()
        {
			setup();
			_periodMonth.SetDaysOff(0);
            _periodMonth.SetDaysOff(999);
        }

        [Test]
        public void VerifyIsDaysOffOverride()
        {
			setup();
			Assert.IsFalse(_periodMonth.IsDaysOffOverride);
            _periodMonth.SetDaysOff(5);
            Assert.IsTrue(_periodMonth.IsDaysOffOverride);
            _periodMonth.ResetDaysOff();
            Assert.IsFalse(_periodMonth.IsDaysOffOverride);
        }

        [Test]
        public void VerifyIsAverageWorkTimePerDayOverride()
        {
			setup();
			Assert.IsFalse(_periodMonth.IsAverageWorkTimePerDayOverride);
			_periodMonth.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6d);
            Assert.IsTrue(_periodMonth.IsAverageWorkTimePerDayOverride);
            _periodMonth.ResetAverageWorkTimePerDay();
            Assert.IsFalse(_periodMonth.IsAverageWorkTimePerDayOverride);
        }

        [Test]
        public void VerifyCanSetFromDate()
        {
			setup();
			DateOnly testDateTime = new DateOnly(DateTime.MinValue);
            _periodMonth.DateFrom = testDateTime;
            Assert.AreEqual(testDateTime, _periodMonth.DateFrom);
        }

        [Test]
        public void VerifyCanSetPeriodType()
        {
			setup();
			_periodMonth.PeriodType = SchedulePeriodType.Week;
            Assert.AreEqual(SchedulePeriodType.Week, _periodMonth.PeriodType);
        }

        [Test]
        public void VerifyCanSetNumber()
        {
			setup();
			_periodMonth.Number = 80;
            Assert.AreEqual(80, _periodMonth.Number);
        }

        [Test]
        public void VerifyNumberGreaterThanZero()
        {
			setup();
			Assert.Throws<ArgumentOutOfRangeException>(() => _periodDay.Number = 0);
        }

        [Test]
        public void VerifyNumberGreaterThanZeroInConstructor()
        {
			setup();
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				new SchedulePeriod(new DateOnly(), SchedulePeriodType.Day, 0);
			});
        }

        [Test]
        public void VerifyCloneWorks()
        {
			setup();
			SchedulePeriod clonedEntity = (SchedulePeriod)_periodMonth.Clone();
            Assert.AreNotEqual(clonedEntity, _periodMonth);
        }

        [Test]
        public void VerifyGetContractScheduleDaysOff()
        {
			setup();
			Assert.AreEqual(3, _periodDay.GetDaysOff(_from));
        }

        [Test]
        public void VerifyGetContractScheduleDaysOffUsesOverride()
        {
            _periodDay.SetDaysOff(5);
            Assert.AreEqual(5, _periodDay.GetDaysOff(_from));
        }

		[Test]
		public void VerifyGetScheduleDaysOffChineseMonth()
		{
			setup();
			_from = new DateOnly(2009, 01, 01);
			Assert.AreEqual(8, _periodChineseMonth.GetDaysOff(_from));
			_from = new DateOnly(2009, 02, 01);
			Assert.AreEqual(7, _periodChineseMonth.GetDaysOff(_from));
			_from = new DateOnly(2009, 04, 01);
			Assert.AreEqual(7, _periodChineseMonth.GetDaysOff(_from));
			_from = new DateOnly(2012, 02, 01);
			Assert.AreEqual(7, _periodChineseMonth.GetDaysOff(_from));
			_from = new DateOnly(2009, 01, 02);
			Assert.AreEqual(8, _periodChineseMonth.GetDaysOff(_from));
			_from = new DateOnly(2009, 01, 31);
			Assert.AreEqual(8, _periodChineseMonth.GetDaysOff(_from));
		}

		//[Test]
		//public void VerifyTargetTimeWhenPersonPeriodStartsInSchedulePeriod()
		//{
		//    _normalPerson.RemoveAllPersonPeriods();
		//    _normalPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2009, 2, 9),
		//                             _personContract, TeamFactory.CreateSimpleTeam()));

		//    _from = new DateOnly(2009,2,2);
		//    _periodMonth = new SchedulePeriod(_from, SchedulePeriodType.Month, 1);
		//    _normalPerson.AddSchedulePeriod(_periodMonth);

		//    int daysOffs = _periodMonth.GetDaysOff(_from);
		//    Assert.AreEqual(6, daysOffs);

		//    DateOnlyPeriod? period = _periodMonth.GetSchedulePeriod(_from);
		//    int daysInPeriod = period.Value.DayCount();
		//    Assert.AreEqual(28, daysInPeriod);

		//    int workDays = _periodMonth.GetWorkdays(_from);

		//    Assert.AreEqual(21 - daysOffs, workDays);
		//}

        [Test]
        public void CanAddShiftCategoryLimitation()
        {
			setup();
			IShiftCategoryLimitation shiftCategoryLimitation =
                new ShiftCategoryLimitation(ShiftCategoryFactory.CreateShiftCategory("xx"));
            _periodMonth.AddShiftCategoryLimitation(shiftCategoryLimitation);
            Assert.AreEqual(1, _periodMonth.ShiftCategoryLimitationCollection().Count);
        }

        [Test]
        public void CannotAddShiftCategoryLimitationWithSameCategory()
        {
			setup();
			Assert.Throws<ArgumentException>(() =>
			{
				IShiftCategoryLimitation shiftCategoryLimitation =
				new ShiftCategoryLimitation(ShiftCategoryFactory.CreateShiftCategory("xx"));
				_periodMonth.AddShiftCategoryLimitation(shiftCategoryLimitation);
				_periodMonth.AddShiftCategoryLimitation(shiftCategoryLimitation);
			});
			
        }

        [Test]
        public void CanRemoveShiftCategoryLimitation()
        {
			setup();
			IShiftCategoryLimitation shiftCategoryLimitation =
                new ShiftCategoryLimitation(ShiftCategoryFactory.CreateShiftCategory("xx"));
            _periodMonth.AddShiftCategoryLimitation(shiftCategoryLimitation);
            _periodMonth.RemoveShiftCategoryLimitation(shiftCategoryLimitation.ShiftCategory);
            Assert.AreEqual(0, _periodMonth.ShiftCategoryLimitationCollection().Count);

            _periodMonth.RemoveShiftCategoryLimitation(shiftCategoryLimitation.ShiftCategory);
            Assert.AreEqual(0, _periodMonth.ShiftCategoryLimitationCollection().Count);
        }

        [Test]
        public void VerifyMustHavePreferences()
        {
			setup();
			_periodMonth.MustHavePreference = _mustHavePreference;
            Assert.AreEqual(_mustHavePreference, _periodMonth.MustHavePreference);
        }

        [Test]
        public void VerifyMustHavePreferencesCannotBeLessThanZero()
        {
			setup();
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				_periodMonth.MustHavePreference = -1;
				Assert.AreEqual(_mustHavePreference, _periodMonth.MustHavePreference);
			});
        }

        [Test]
        public void VerifyBalanceIn()
        {
			setup();
			TimeSpan value = new TimeSpan();
            _periodDay.BalanceIn = value;
            Assert.AreEqual(value, _periodDay.BalanceIn);
        }

        [Test]
        public void VerifyExtra()
        {
			setup();
			TimeSpan value = new TimeSpan();
            _periodDay.Extra = value;
            Assert.AreEqual(value, _periodDay.Extra);
        }

        [Test]
        public void VerifyBalanceOut()
        {
			setup();
			TimeSpan value = new TimeSpan();
            _periodDay.BalanceOut = value;
            Assert.AreEqual(value, _periodDay.BalanceOut);
        }

		[Test]
		public void ShouldReturnRealEndDate()
		{
			setup();
			var startDate = new DateOnly(2011, 1, 1);
			var endDateDay = new DateOnly(2011, 1, 10);
			var endDateWeek = new DateOnly(2011, 1, 14);
			var endDateMonth = new DateOnly(2011, 1, 31);
			var schedulePeriodDay = new SchedulePeriod(startDate, SchedulePeriodType.Day, 10);
			var schedulePeriodWeek = new SchedulePeriod(startDate, SchedulePeriodType.Week, 2);
			var schedulePeriodMonth = new SchedulePeriod(startDate, SchedulePeriodType.Month, 1);

			Assert.AreEqual(endDateDay, schedulePeriodDay.RealDateTo());
			Assert.AreEqual(endDateWeek, schedulePeriodWeek.RealDateTo());
			Assert.AreEqual(endDateMonth, schedulePeriodMonth.RealDateTo());
		}

		[Test]
		public void BalanceInShouldRoundToMinutes()
		{
			setup();
			_periodWeek.BalanceIn = TimeSpan.FromMinutes(1.5);
			Assert.AreEqual(TimeSpan.FromMinutes(2), _periodWeek.BalanceIn);

		}

		[Test]
		public void BalanceOutShouldRoundToMinutes()
		{
			setup();
			_periodWeek.BalanceOut = TimeSpan.FromMinutes(1.5);
			Assert.AreEqual(TimeSpan.FromMinutes(2), _periodWeek.BalanceOut);

		}

		[Test]
		public void ExtraShouldRoundToMinutes()
		{
			setup();
			_periodWeek.Extra = TimeSpan.FromMinutes(1.5);
			Assert.AreEqual(TimeSpan.FromMinutes(2), _periodWeek.Extra);

		}
    }
}
