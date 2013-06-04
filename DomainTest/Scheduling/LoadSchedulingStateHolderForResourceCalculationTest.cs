using System.Collections.Generic;
using Rhino.Mocks;
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
		private MockRepository _mocks;
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
			_mocks = new MockRepository();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_peopleAndSkillLoadDecider = _mocks.DynamicMock<IPeopleAndSkillLoaderDecider>();
			_skillDayLoadHelper = _mocks.DynamicMock<ISkillDayLoadHelper>();
			_personRepository = _mocks.DynamicMock<IPersonRepository>();
			_personAbsenceAccountRepository = _mocks.DynamicMock<IPersonAbsenceAccountRepository>();
			_skillRepository = _mocks.DynamicMock<ISkillRepository>();
			_workloadRepository = _mocks.DynamicMock<IWorkloadRepository>();
			_scheduleRepository = _mocks.DynamicMock<IScheduleRepository>();
			_personProvider = _mocks.DynamicMock<IPersonProvider>();
			_target = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository, _skillRepository, _workloadRepository, _scheduleRepository, _schedulingResultStateHolder, _peopleAndSkillLoadDecider, _skillDayLoadHelper, p => _personProvider);
		}

		[Test]
		public void ShouldLoadPersonAccountsOnExecute()
		{
			var accounts = _mocks.Stub<IDictionary<IPerson, IPersonAccountCollection>>();

			Expect.Call(_personAbsenceAccountRepository.FindByUsers(null)).Return(accounts).IgnoreArguments();
			_schedulingResultStateHolder.AllPersonAccounts = accounts;

			_mocks.ReplayAll();

			_target.Execute(null, new DateTimePeriod(2010, 2, 1, 2010, 2, 2), new IPerson[]{});

			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyExecute()
		{
			DateTimePeriod period = new DateTimePeriod(2010,2,1,2010,2,2);
			IScenario scenario = _mocks.StrictMock<IScenario>();
			IPerson person = PersonFactory.CreatePerson();
			IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			IPersonProvider personsInOrganizationProvider = _mocks.StrictMock<IPersonProvider>();
		    IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();

			var skills = new List<ISkill> {SkillFactory.CreateSkill("test")};
			var requestedPeople = new List<IPerson> {person};
			var peopleInOrganization = new List<IPerson> {person};
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());

			using (_mocks.Record())
			{
				Expect.Call(_workloadRepository.LoadAll()).Return(new List<IWorkload>());
				Expect.Call(_personRepository.FindPeopleInOrganization(dateOnlyPeriod, false)).Return(peopleInOrganization);
				Expect.Call(_scheduleRepository.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, scheduleDictionaryLoadOptions, null)).IgnoreArguments
					().Return(scheduleDictionary);
				Expect.Call(_skillRepository.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
				_peopleAndSkillLoadDecider.Execute(scenario,period,requestedPeople);
				Expect.Call(_peopleAndSkillLoadDecider.FilterPeople(peopleInOrganization)).Return(0);
				Expect.Call(_peopleAndSkillLoadDecider.FilterSkills(skills)).Return(0);
				Expect.Call(_skillDayLoadHelper.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario)).Return(
																		new Dictionary<ISkill, IList<ISkillDay>>());
			}
			using (_mocks.Playback())
			{
				_target.Execute(scenario, period, requestedPeople);
			}
		}
	
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldLoadSchedulesForRequest()
		{
			var period = new DateTimePeriod(2010,2,1,2010,2,2);
			var scenario = _mocks.StrictMock<IScenario>();
			var person = PersonFactory.CreatePerson();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var personsInOrganizationProvider = _mocks.StrictMock<IPersonProvider>();
		    var scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();

			var skills = new List<ISkill> {SkillFactory.CreateSkill("test")};
			var requestedPeople = new List<IPerson> {person};
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());

			using (_mocks.Record())
			{
				Expect.Call(_workloadRepository.LoadAll()).Return(new List<IWorkload>());
				Expect.Call(_scheduleRepository.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, scheduleDictionaryLoadOptions, null)).IgnoreArguments
					().Return(scheduleDictionary);
				Expect.Call(_skillRepository.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
				_peopleAndSkillLoadDecider.Execute(scenario,period,requestedPeople);
				Expect.Call(_peopleAndSkillLoadDecider.FilterSkills(skills)).Return(0);
				Expect.Call(_skillDayLoadHelper.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario)).Return(
																		new Dictionary<ISkill, IList<ISkillDay>>());
			}
			using (_mocks.Playback())
			{
				_target.LoadForRequest(scenario, period, requestedPeople);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyExecuteWithAllPersonsFilteredAway()
		{
			DateTimePeriod period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
			IScenario scenario = _mocks.StrictMock<IScenario>();
			IPerson person = PersonFactory.CreatePerson();
			IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			IPersonProvider personsInOrganizationProvider = _mocks.StrictMock<IPersonProvider>();
		    IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();

			var skills = new List<ISkill> { SkillFactory.CreateSkill("test") }; 
			var requestedPeople = new List<IPerson> { person };
			var peopleInOrganization = new List<IPerson>();
			var visiblePeople = new List<IPerson>();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());

			using (_mocks.Record())
			{
				Expect.Call(_workloadRepository.LoadAll()).Return(new List<IWorkload>());
				Expect.Call(_personRepository.FindPeopleInOrganization(dateOnlyPeriod, false)).Return(peopleInOrganization);
				Expect.Call(_scheduleRepository.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, scheduleDictionaryLoadOptions, visiblePeople)).IgnoreArguments
					().Return(scheduleDictionary);
				Expect.Call(_skillRepository.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
				_peopleAndSkillLoadDecider.Execute(scenario, period, requestedPeople);
				Expect.Call(_peopleAndSkillLoadDecider.FilterPeople(peopleInOrganization)).Return(1);
				Expect.Call(_peopleAndSkillLoadDecider.FilterSkills(skills)).Return(0);
				Expect.Call(_skillDayLoadHelper.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario)).Return(
																		new Dictionary<ISkill, IList<ISkillDay>>());
			}
			using (_mocks.Playback())
			{
				Assert.AreEqual(0, _schedulingResultStateHolder.PersonsInOrganization.Count);
				_target.Execute(scenario, period, requestedPeople);
				Assert.AreEqual(1, _schedulingResultStateHolder.PersonsInOrganization.Count);
				Assert.IsTrue(_schedulingResultStateHolder.PersonsInOrganization.Contains(person));
			}
		}
	}
}
