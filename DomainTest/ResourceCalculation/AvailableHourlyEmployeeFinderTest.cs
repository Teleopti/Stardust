using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class AvailableHourlyEmployeeFinderTest
	{
		private IAvailableHourlyEmployeeFinder _target;
		private IPerson _sourcePerson;
		private DateOnly _date;
		private ScheduleDictionaryForTest _dic;
		private IScenario _scenario;
		private DateTimePeriod _dtp;
		private DateOnly _dateOnly;
		private ITeam _team;
		private IPartTimePercentage _percentage;
		private IContractSchedule _contractSchedule;
	    private MockRepository _mocks;
	    private ISchedulingResultStateHolder _stateHolder;

	    [SetUp]
		public void Setup()
		{
		    _mocks = new MockRepository();
			_dtp = new DateTimePeriod(2011, 1, 1, 2011, 1, 10);
			IScheduleDateTimePeriod period = new ScheduleDateTimePeriod(_dtp);
			_scenario = ScenarioFactory.CreateScenarioAggregate();
	        _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_dic = new ScheduleDictionaryForTest(_scenario, period,
			                                     new Dictionary<IPerson, IScheduleRange>());
			_sourcePerson = PersonFactory.CreatePersonWithGuid("source", "source");
			IList<IPerson> filteredPersons = new List<IPerson> {_sourcePerson};
			_date = new DateOnly(2011,1,5);
            _target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()),  _sourcePerson, _date, _stateHolder, filteredPersons);
			IScheduleParameters scheduleParameters = new ScheduleParameters(_scenario, _sourcePerson, _dtp);
			IScheduleRange range = new ScheduleRange(_dic, scheduleParameters);
			_dic.AddTestItem(_sourcePerson, range);

			_dateOnly = DateOnly.MinValue;
			_team = TeamFactory.CreateSimpleTeam();
			_percentage = PartTimePercentageFactory.CreatePartTimePercentage("hej");
			_contractSchedule = ContractScheduleFactory.CreateContractSchedule("hopp");
		}

		[Test]
		public void ShouldReturnEmptyListIfSourceNotScheduled()
		{
			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void ShouldReturnOnlyHourlyEmployees()
		{
			schedulePerson(_sourcePerson);

			IPerson fixedPerson =  createFixedPerson();
			IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			IPerson extraPerson = createExtraPerson(restriction);

			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson, fixedPerson, extraPerson };
			_target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date, _stateHolder, filteredPersons);

            Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.AtLeastOnce();
            _mocks.ReplayAll();
			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(extraPerson, result[0].Person);
			Assert.IsNotNull(result[0].WorkTimesTomorrow);
			Assert.IsNotNull(result[0].WorkTimesYesterday);
			Assert.IsNotNull(result[0].Availability);
            _mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotReturnScheduledEmployee()
		{
			schedulePerson(_sourcePerson);

			IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			IPerson extraPerson = createExtraPerson(restriction);
			IPerson extraPerson2 = createExtraPerson(restriction);
			schedulePerson(extraPerson);

			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson, extraPerson, extraPerson2 };
			_target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date, _stateHolder, filteredPersons);
            Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(extraPerson2, result[0].Person);
			Assert.That(result[0].Matching, Is.Not.Null);
			Assert.That(result[0].NightRestOk, Is.True);
            _mocks.VerifyAll();
		}

		[Test]
		public void ShouldAlsoReturnPersonWithNoMatchingStudentAvailability()
		{
			schedulePerson(_sourcePerson);

			IPerson extraPerson = createExtraPerson(null);

			IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			IPerson extraPerson3 = createExtraPerson(restriction);
			
			restriction = new StudentAvailabilityRestriction();
			restriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(12), null);
			IPerson extraPerson2 = createExtraPerson(restriction);
			
			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson, extraPerson, extraPerson2, extraPerson3 };
			_target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date, _stateHolder, filteredPersons);

            Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.AtLeastOnce();
            _mocks.ReplayAll();
			IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
			Assert.AreEqual(3, result.Count);

            _mocks.VerifyAll();
		}

		[Test]
		public void ShouldCheckNightRest()
		{
			schedulePerson(_sourcePerson);
			IPerson extraPerson = createExtraPerson(null);
			scheduleExtraPerson(extraPerson);
			IList<IPerson> filteredPersons = new List<IPerson> { _sourcePerson, extraPerson };

			_target = new AvailableHourlyEmployeeFinder(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), _sourcePerson, _date, _stateHolder, filteredPersons);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				IList<AvailableHourlyEmployeeFinderResult> result = _target.Find();
				Assert.IsTrue(result[0].NightRestOk);
			}
      
      
		}

		private void schedulePerson(IPerson person)
		{
			DateTime start = DateTime.SpecifyKind(_date.Date.AddHours(8), DateTimeKind.Utc);
			DateTime end = DateTime.SpecifyKind(_date.Date.AddHours(16), DateTimeKind.Utc);
			DateTimePeriod shiftPeriod = new DateTimePeriod(start, end);
			var mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("act"), shiftPeriod,
																	ShiftCategoryFactory.CreateShiftCategory("bla"));
			IScheduleDay scheduleDay = _dic[person].ScheduledDay(_date);
			scheduleDay.AddMainShift(mainShift);
            _dic.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay> { scheduleDay }, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		private void scheduleExtraPerson(IPerson person)
		{
			DateTime start = DateTime.SpecifyKind(_date.Date.AddDays(-1).AddHours(8), DateTimeKind.Utc);
			DateTime end = DateTime.SpecifyKind(_date.Date.AddDays(-1).AddHours(16), DateTimeKind.Utc);
			DateTimePeriod shiftPeriod = new DateTimePeriod(start, end);
			var mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("act"), shiftPeriod,
																	ShiftCategoryFactory.CreateShiftCategory("bla"));
			IScheduleDay scheduleDay = _dic[person].ScheduledDay(_date.AddDays(-1));
			scheduleDay.AddMainShift(mainShift);
			_dic.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay> { scheduleDay }, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));

			start = DateTime.SpecifyKind(_date.Date.AddDays(1).AddHours(8), DateTimeKind.Utc);
			end = DateTime.SpecifyKind(_date.Date.AddDays(1).AddHours(16), DateTimeKind.Utc);
			shiftPeriod = new DateTimePeriod(start, end);
			mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("act"), shiftPeriod,
																	ShiftCategoryFactory.CreateShiftCategory("bla"));
			scheduleDay = _dic[person].ScheduledDay(_date.AddDays(1));
			scheduleDay.AddMainShift(mainShift);
			_dic.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay> { scheduleDay }, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		private IPerson createFixedPerson()
		{
			IPerson fixedPerson = PersonFactory.CreatePersonWithGuid("fixed", "fixed");
			IContract fixedContract = ContractFactory.CreateContract("hepp");
			fixedContract.EmploymentType = EmploymentType.FixedStaffNormalWorkTime;
			IPersonContract fixedPersonContract = PersonContractFactory.CreatePersonContract(fixedContract,
																							 _percentage, _contractSchedule);
			fixedPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(_dateOnly, fixedPersonContract, _team));
			IScheduleParameters scheduleParameters = new ScheduleParameters(_scenario, fixedPerson, _dtp);
			IScheduleRange range = new ScheduleRange(_dic, scheduleParameters);
			_dic.AddTestItem(fixedPerson, range);

			return fixedPerson;
		}

		private IPerson createExtraPerson(IStudentAvailabilityRestriction restriction)
		{
			IPerson extraPerson = PersonFactory.CreatePersonWithGuid("extraPerson", "extraPerson");
			IContract fixedContract = ContractFactory.CreateContract("hupp");
			fixedContract.EmploymentType = EmploymentType.HourlyStaff;
			IPersonContract fixedPersonContract = PersonContractFactory.CreatePersonContract(fixedContract,
																							 _percentage, _contractSchedule);
			extraPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(_dateOnly, fixedPersonContract, _team));
			IScheduleParameters scheduleParameters = new ScheduleParameters(_scenario, extraPerson, _dtp);
			IScheduleRange range = new ScheduleRange(_dic, scheduleParameters);
			_dic.AddTestItem(extraPerson, range);

			if(restriction != null)
			{
				IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(extraPerson, _date, new List<IStudentAvailabilityRestriction> { restriction });
				IScheduleDay scheduleDay = _dic[extraPerson].ScheduledDay(_date);
				scheduleDay.Add(studentAvailabilityDay);
                _dic.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay> { scheduleDay }, NewBusinessRuleCollection.Minimum(), new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
			}

			return extraPerson;
		}

	}
}