using System.Collections.Generic;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;


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
		private IScheduleStorage _scheduleStorage;

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
			_scheduleStorage = MockRepository.GenerateMock<IScheduleStorage>();
			_target = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository, _skillRepository, _workloadRepository, _scheduleStorage, _peopleAndSkillLoadDecider, _skillDayLoadHelper);
		}

		[Test]
		public void ShouldLoadPersonAccountsOnExecute()
		{
			var accounts = new Dictionary<IPerson, IPersonAccountCollection>();
			var period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
			var people = new IPerson[]{};
			
			_personAbsenceAccountRepository.Stub(x => x.FindByUsers(null)).Return(accounts).IgnoreArguments();
			_peopleAndSkillLoadDecider.Stub(x => x.Execute(null, period, people));
			_skillRepository.Stub(x => x.FindAllWithSkillDays(new DateOnlyPeriod())).Return(new List<ISkill>()).IgnoreArguments();
			_schedulingResultStateHolder.AllPersonAccounts = accounts;

			_target.Execute(null, period, people, _schedulingResultStateHolder, loadLight: false);

			_personAbsenceAccountRepository.AssertWasCalled(x => x.FindByUsers(null), o => o.IgnoreArguments());
		}

		[Test]
		public void VerifyExecute()
		{
			var period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IPerson person = PersonFactory.CreatePerson();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false,false);

			var skills = new List<ISkill> {SkillFactory.CreateSkill("test")};
			var requestedPeople = new List<IPerson> {person};
			var peopleInOrganization = new List<IPerson> {person};
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
			var skillDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>{{skills[0],new ISkillDay[]{}}};
			
			_workloadRepository.Stub(x => x.LoadAll()).Return(new List<IWorkload>());
			_personRepository.Stub(x => x.FindAllAgents(dateOnlyPeriod, false)).Return(peopleInOrganization);
			_scheduleStorage.Stub(
				x => x.FindSchedulesForPersons(scenario, _schedulingResultStateHolder.LoadedAgents, scheduleDictionaryLoadOptions, new DateTimePeriod(), null, false))
				.IgnoreArguments()
				.Return(scheduleDictionary);
			_peopleAndSkillLoadDecider.Stub(x => x.Execute(scenario, period, peopleInOrganization)).Return(MockRepository.GenerateMock<ILoaderDeciderResult>());
			_skillRepository.Stub(x => x.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
			_skillDayLoadHelper.Stub(x => x.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario))
				.Return(skillDictionary).IgnoreArguments();

			_target.Execute(scenario, period, requestedPeople, _schedulingResultStateHolder, loadLight: false);

			_schedulingResultStateHolder.Schedules.Should().Be.SameInstanceAs(scheduleDictionary);
			_schedulingResultStateHolder.SkillDays.Should().Be.SameInstanceAs(skillDictionary);
		}

		[Test]
		public void VerifyExecuteWithAllPersonsFilteredAway()
		{
			var period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
			IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
			IPerson person = PersonFactory.CreatePerson();
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false,false);

			var skills = new List<ISkill> {SkillFactory.CreateSkill("test")};
			var requestedPeople = new List<IPerson> {person};
			var peopleInOrganization = new List<IPerson>();
			var visiblePeople = new List<IPerson>();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());

			_workloadRepository.Stub(x => x.LoadAll()).Return(new List<IWorkload>());
			_personRepository.Stub(x => x.FindAllAgents(dateOnlyPeriod, false)).Return(peopleInOrganization);
			_scheduleStorage.Stub(
				x =>
					x.FindSchedulesForPersons(scenario, _schedulingResultStateHolder.LoadedAgents, scheduleDictionaryLoadOptions,
						new DateTimePeriod(), visiblePeople, false)).IgnoreArguments().Return(scheduleDictionary);
			_skillRepository.Stub(x => x.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
			_peopleAndSkillLoadDecider.Stub(x => x.Execute(scenario, period, requestedPeople))
				.Return(MockRepository.GenerateMock<ILoaderDeciderResult>());
			_skillDayLoadHelper.Stub(x => x.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario))
				.Return(new Dictionary<ISkill, IEnumerable<ISkillDay>>());

			Assert.AreEqual(0, _schedulingResultStateHolder.LoadedAgents.Count);
			_target.Execute(scenario, period, requestedPeople, _schedulingResultStateHolder, loadLight: false);

			_peopleAndSkillLoadDecider.AssertWasCalled(x => x.Execute(scenario, period, requestedPeople));
			Assert.AreEqual(1, _schedulingResultStateHolder.LoadedAgents.Count);
			Assert.IsTrue(_schedulingResultStateHolder.LoadedAgents.Contains(person));
		}
	}
}
