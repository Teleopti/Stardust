using System.Collections.Generic;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class LoadSchedulingStateHolderForResourceCalculationTest
	{
		private LoadSchedulingStateHolderForResourceCalculation _target;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPeopleAndSkillLoaderDecider _peopleAndSkillLoadDecider;
		private ISkillDayLoadHelper _skillDayLoadHelper;
		private IPersonRepository _personRepository;
		private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private ISkillRepository _skillRepository;
		private IWorkloadRepository _workloadRepository;
		private IScheduleRepository _scheduleRepository;
		private IPersonProvider _personProvider;

		[SetUp]
		public void Setup()
		{
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_peopleAndSkillLoadDecider = MockRepository.GenerateMock<IPeopleAndSkillLoaderDecider>();
			_skillDayLoadHelper = MockRepository.GenerateMock<ISkillDayLoadHelper>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_personAbsenceAccountRepository = MockRepository.GenerateMock<IPersonAbsenceAccountRepository>();
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			_scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			_personProvider = MockRepository.GenerateMock<IPersonProvider>();
			_target = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository, _skillRepository, _workloadRepository, _scheduleRepository, _schedulingResultStateHolder, _peopleAndSkillLoadDecider, _skillDayLoadHelper, p => _personProvider);
		}

		[Test]
		public void ShouldLoadPersonAccountsOnExecute()
		{
			var accounts = new Dictionary<IPerson, IPersonAccountCollection>();

			_personAbsenceAccountRepository.Stub(x => x.FindByUsers(null)).Return(accounts).IgnoreArguments();
			_schedulingResultStateHolder.AllPersonAccounts = accounts;

			_target.Execute(null, new DateTimePeriod(2010, 2, 1, 2010, 2, 2), new IPerson[]{});

			_personAbsenceAccountRepository.AssertWasCalled(x => x.FindByUsers(null), o => o.IgnoreArguments());
		}

		[Test]
		public void VerifyExecute()
		{
			var period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IPerson person = PersonFactory.CreatePerson();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var personsInOrganizationProvider = MockRepository.GenerateMock<IPersonProvider>();
			var scheduleDictionaryLoadOptions = MockRepository.GenerateMock<IScheduleDictionaryLoadOptions>();

			var skills = new List<ISkill> {SkillFactory.CreateSkill("test")};
			var requestedPeople = new List<IPerson> {person};
			var peopleInOrganization = new List<IPerson> {person};
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var skillDictionary = new Dictionary<ISkill, IList<ISkillDay>>{{skills[0],new ISkillDay[]{}}};
			
			_workloadRepository.Stub(x => x.LoadAll()).Return(new List<IWorkload>());
			_personRepository.Stub(x => x.FindPeopleInOrganization(dateOnlyPeriod, false)).Return(peopleInOrganization);
			_scheduleRepository.Stub(
				x => x.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, scheduleDictionaryLoadOptions, null))
				.IgnoreArguments()
				.Return(scheduleDictionary);
			_skillRepository.Stub(x => x.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
			_peopleAndSkillLoadDecider.Stub(x => x.FilterPeople(peopleInOrganization)).Return(0);
			_peopleAndSkillLoadDecider.Stub(x => x.FilterSkills(skills)).Return(0);
			_skillDayLoadHelper.Stub(x => x.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario))
				.Return(skillDictionary).IgnoreArguments();

			_target.Execute(scenario, period, requestedPeople);

			_schedulingResultStateHolder.Schedules.Should().Be.SameInstanceAs(scheduleDictionary);
			_schedulingResultStateHolder.SkillDays.Should().Be.Equals(skillDictionary);
		}

		[Test]
		public void ShouldNotLoadRestrictionAndNotes()
		{
			var period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IPerson person = PersonFactory.CreatePerson();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var personsInOrganizationProvider = MockRepository.GenerateMock<IPersonProvider>();

			var skills = new List<ISkill> { SkillFactory.CreateSkill("test") };
			var requestedPeople = new List<IPerson> { person };
			var peopleInOrganization = new List<IPerson> { person };
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var skillDictionary = new Dictionary<ISkill, IList<ISkillDay>> { { skills[0], new ISkillDay[] { } } };

			_workloadRepository.Stub(x => x.LoadAll()).Return(new List<IWorkload>());
			_personRepository.Stub(x => x.FindPeopleInOrganization(dateOnlyPeriod, false)).Return(peopleInOrganization);
			_scheduleRepository.Stub(
				x => x.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, null, null))
				.IgnoreArguments()
				.Return(scheduleDictionary);
			_skillRepository.Stub(x => x.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
			_peopleAndSkillLoadDecider.Stub(x => x.FilterPeople(peopleInOrganization)).Return(0);
			_peopleAndSkillLoadDecider.Stub(x => x.FilterSkills(skills)).Return(0);
			_skillDayLoadHelper.Stub(x => x.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario))
				.Return(skillDictionary);

			_target.Execute(scenario, period, requestedPeople);

			_scheduleRepository.AssertWasCalled(
				x => x.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, null, null),
				o =>
					o.Constraints(Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Same(scenario),
						Rhino.Mocks.Constraints.Is.Anything(),
						new Rhino.Mocks.Constraints.PredicateConstraint<IScheduleDictionaryLoadOptions>(
							x => x.LoadNotes == false && x.LoadRestrictions == false), Rhino.Mocks.Constraints.Is.Anything()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyExecuteWithAllPersonsFilteredAway()
		{
			var period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IPerson person = PersonFactory.CreatePerson();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var personsInOrganizationProvider = MockRepository.GenerateMock<IPersonProvider>();
			var scheduleDictionaryLoadOptions = MockRepository.GenerateMock<IScheduleDictionaryLoadOptions>();

			var skills = new List<ISkill> { SkillFactory.CreateSkill("test") }; 
			var requestedPeople = new List<IPerson> { person };
			var peopleInOrganization = new List<IPerson>();
			var visiblePeople = new List<IPerson>();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());

				_workloadRepository.Stub(x => x.LoadAll()).Return(new List<IWorkload>());
				_personRepository.Stub(x => x.FindPeopleInOrganization(dateOnlyPeriod, false)).Return(peopleInOrganization);
				_scheduleRepository.Stub(x => x.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, scheduleDictionaryLoadOptions, visiblePeople)).IgnoreArguments().Return(scheduleDictionary);
				_skillRepository.Stub(x => x.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
				_peopleAndSkillLoadDecider.Stub(x => x.FilterPeople(peopleInOrganization)).Return(1);
				_peopleAndSkillLoadDecider.Stub(x => x.FilterSkills(skills)).Return(0);
				_skillDayLoadHelper.Stub(x => x.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario)).Return(new Dictionary<ISkill, IList<ISkillDay>>());
			
				Assert.AreEqual(0, _schedulingResultStateHolder.PersonsInOrganization.Count);
				_target.Execute(scenario, period, requestedPeople);
				
			_peopleAndSkillLoadDecider.AssertWasCalled(x => x.Execute(scenario, period, requestedPeople));
			Assert.AreEqual(1, _schedulingResultStateHolder.PersonsInOrganization.Count);
				Assert.IsTrue(_schedulingResultStateHolder.PersonsInOrganization.Contains(person));
		}
	}
}
