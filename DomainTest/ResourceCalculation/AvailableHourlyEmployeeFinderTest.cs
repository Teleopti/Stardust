using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class AvailableHourlyEmployeeFinderTest
	{
		private IAvailableHourlyEmployeeFinder _target;
		private IPerson _sourcePerson;
		private DateOnly _date = new DateOnly(2011,1,5);
		private readonly DateOnly _dateOnly = DateOnly.MinValue;
		private ITeam _team;
		private IPartTimePercentage _percentage;
		private IContractSchedule _contractSchedule;
		private IScheduleDay _scheduleDay;

		[SetUp]
		public void Setup()
		{
			_sourcePerson = PersonFactory.CreatePersonWithGuid("source", "source");
			_scheduleDay = ScheduleDayFactory.Create(_date, _sourcePerson);
			
			_team = TeamFactory.CreateSimpleTeam();
			_percentage = PartTimePercentageFactory.CreatePartTimePercentage("hej");
			_contractSchedule = ContractScheduleFactory.CreateContractSchedule("hopp");
		}

		[Test]
		public void ShouldReturnEmptyListIfSourceNotScheduled()
		{
			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson };

			_target =
				new AvailableHourlyEmployeeFinder(
					new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date,
					new FakeScheduleDayForPerson(_scheduleDay), filteredPersons, UserTimeZone.Make());
			
			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void ShouldReturnOnlyHourlyEmployees()
		{
			scheduleDay(_scheduleDay);

			IPerson fixedPerson =  createFixedPerson();
			IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			IPerson extraPerson = createExtraPerson();
			var extraSchedule = createScheduleDayFromRestriction(extraPerson, restriction);

			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson, fixedPerson, extraPerson };
			_target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date, new FakeScheduleDayForPerson(_scheduleDay, extraSchedule), filteredPersons, UserTimeZone.Make());

			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(extraPerson, result[0].Person);
			Assert.IsNotNull(result[0].WorkTimesTomorrow);
			Assert.IsNotNull(result[0].WorkTimesYesterday);
			Assert.IsNotNull(result[0].Availability);
		}

		[Test]
		public void ShouldNotReturnScheduledEmployee()
		{
			scheduleDay(_scheduleDay);

			IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			IPerson extraPerson = createExtraPerson();
			var extraSchedules = createScheduleDayFromRestriction(extraPerson, restriction);
			IPerson extraPerson2 = createExtraPerson();
			var extraSchedules2 = createScheduleDayFromRestriction(extraPerson2, restriction);
			scheduleDay(extraSchedules);

			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson, extraPerson, extraPerson2 };
			_target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date, new FakeScheduleDayForPerson(_scheduleDay, extraSchedules, extraSchedules2), filteredPersons, UserTimeZone.Make());
            
			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(extraPerson2, result[0].Person);
			Assert.That(result[0].Matching, Is.Not.Null);
			Assert.That(result[0].NightRestOk, Is.True);
		}

		[Test]
		public void ShouldAlsoReturnPersonWithNoMatchingStudentAvailability()
		{
			scheduleDay(_scheduleDay);

			IPerson extraPerson = createExtraPerson();

			IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			var extraPerson3 = createExtraPerson();
			var extraSchedules3 = createScheduleDayFromRestriction(extraPerson3, restriction);

			restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(12), null);
			var extraPerson2 = createExtraPerson();
			var extraSchedules2 = createScheduleDayFromRestriction(extraPerson2, restriction);
			
			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson, extraPerson, extraPerson2, extraPerson3 };
			_target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date, new FakeScheduleDayForPerson(_scheduleDay, extraSchedules2, extraSchedules3), filteredPersons, UserTimeZone.Make());

			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(3, result.Count);
		}

		[Test]
		public void ShouldCheckNightRest()
		{
			scheduleDay(_scheduleDay);
			var extraPerson = createExtraPerson();
			var schedules = scheduleExtraPerson(extraPerson);
			IList<IPerson> filteredPersons = new List<IPerson> {_sourcePerson, extraPerson};

			Array.Resize(ref schedules, schedules.Length + 1);
			schedules[schedules.Length - 1] = _scheduleDay;

			_target =
				new AvailableHourlyEmployeeFinder(
					new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date,
					new FakeScheduleDayForPerson(schedules), filteredPersons, new SpecificTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo()));

			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.IsTrue(result[0].NightRestOk);
		}

		private void scheduleDay(IScheduleDay scheduleDay)
		{
			DateTime start = DateTime.SpecifyKind(_date.Date.AddHours(8), DateTimeKind.Utc);
			DateTime end = DateTime.SpecifyKind(_date.Date.AddHours(16), DateTimeKind.Utc);
			DateTimePeriod shiftPeriod = new DateTimePeriod(start, end);
			var mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("act"), shiftPeriod,
																	ShiftCategoryFactory.CreateShiftCategory("bla"));
			scheduleDay.AddMainShift(mainShift);
		}

		private IScheduleDay[] scheduleExtraPerson(IPerson person)
		{
			DateTime start = DateTime.SpecifyKind(_date.Date.AddDays(-1).AddHours(8), DateTimeKind.Utc);
			DateTime end = DateTime.SpecifyKind(_date.Date.AddDays(-1).AddHours(16), DateTimeKind.Utc);
			DateTimePeriod shiftPeriod = new DateTimePeriod(start, end);
			var mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("act"), shiftPeriod,
																	ShiftCategoryFactory.CreateShiftCategory("bla"));

			var scheduleDayBefore = ScheduleDayFactory.Create(_date.AddDays(-1), person);
			scheduleDayBefore.AddMainShift(mainShift);
			
			start = DateTime.SpecifyKind(_date.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);
			end = DateTime.SpecifyKind(_date.Date.AddDays(1).AddHours(16), DateTimeKind.Utc);
			shiftPeriod = new DateTimePeriod(start, end);
			mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("act"), shiftPeriod,
																	ShiftCategoryFactory.CreateShiftCategory("bla"));
			var scheduleDayAfter = ScheduleDayFactory.Create(_date.AddDays(1), person);
			_scheduleDay.AddMainShift(mainShift);

			return new[] {scheduleDayBefore, _scheduleDay, scheduleDayAfter};
		}

		private IPerson createFixedPerson()
		{
			IPerson fixedPerson = PersonFactory.CreatePersonWithGuid("fixed", "fixed");
			IContract fixedContract = ContractFactory.CreateContract("hepp");
			fixedContract.EmploymentType = EmploymentType.FixedStaffNormalWorkTime;
			IPersonContract fixedPersonContract = PersonContractFactory.CreatePersonContract(fixedContract,
																							 _percentage, _contractSchedule);
			fixedPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(_dateOnly, fixedPersonContract, _team));
			
			return fixedPerson;
		}

		private IScheduleDay createScheduleDayFromRestriction(IPerson person, IStudentAvailabilityRestriction restriction)
		{
			IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(person, _date, new List<IStudentAvailabilityRestriction> { restriction });
			IScheduleDay scheduleDay = ScheduleDayFactory.Create(_date, person);
			scheduleDay.Add(studentAvailabilityDay);
			
			return scheduleDay;
		}

		private IPerson createExtraPerson()
		{
			IPerson extraPerson = PersonFactory.CreatePersonWithGuid("extraPerson", "extraPerson");
			IContract fixedContract = ContractFactory.CreateContract("hupp");
			fixedContract.EmploymentType = EmploymentType.HourlyStaff;
			IPersonContract fixedPersonContract = PersonContractFactory.CreatePersonContract(fixedContract,
																							 _percentage, _contractSchedule);
			extraPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(_dateOnly, fixedPersonContract, _team));
			
			return extraPerson;
		}
	}
}