using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class ResourceCalculateSkillCommandTest
    {
        private ResourceCalculateSkillCommand _target;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private ISkillLoaderDecider _skillLoadDecider;
        private ISkillDayLoadHelper _skillDayLoadHelper;
        private ISkillRepository _skillRepository;
        private IWorkloadRepository _workloadRepository;
        private IScheduleRepository _scheduleRepository;
	    private IScheduledResourcesReadModelStorage _storage;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = new SchedulingResultStateHolder();
            _skillLoadDecider = _mocks.DynamicMock<ISkillLoaderDecider>();
            _skillDayLoadHelper = _mocks.DynamicMock<ISkillDayLoadHelper>();
            _skillRepository = _mocks.DynamicMock<ISkillRepository>();
            _workloadRepository = _mocks.DynamicMock<IWorkloadRepository>();
            _scheduleRepository = _mocks.DynamicMock<IScheduleRepository>();
            _storage = _mocks.DynamicMock<IScheduledResourcesReadModelStorage>();
            _target = new ResourceCalculateSkillCommand(_skillRepository, _workloadRepository, _storage, _schedulingResultStateHolder, _skillLoadDecider, _skillDayLoadHelper);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyExecute()
        {
            var period = new DateTimePeriod(2010, 2, 1, 2010, 2, 2);
            var scenario = _mocks.StrictMock<IScenario>();
            var person = PersonFactory.CreatePerson();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var personsInOrganizationProvider = _mocks.StrictMock<IPersonProvider>();
            var scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();

            var skills = new List<ISkill> { SkillFactory.CreateSkill("test") };
            var skill = SkillFactory.CreateSkill("test");
            var peopleInOrganization = new List<IPerson> { person };
            var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());

            using (_mocks.Record())
            {
                Expect.Call(_workloadRepository.LoadAll()).Return(new List<IWorkload>());
                Expect.Call(_scheduleRepository.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, scheduleDictionaryLoadOptions, null)).IgnoreArguments
                    ().Return(scheduleDictionary);
                Expect.Call(_skillRepository.FindAllWithSkillDays(dateOnlyPeriod)).Return(skills);
                _skillLoadDecider.Execute(scenario, period, skill);
                Expect.Call(_skillLoadDecider.FilterPeople(peopleInOrganization)).Return(0);
                Expect.Call(_skillLoadDecider.FilterSkills(skills)).Return(0);
                Expect.Call(_skillDayLoadHelper.LoadSchedulerSkillDays(period.ToDateOnlyPeriod(skills[0].TimeZone), skills, scenario)).Return(
                    new Dictionary<ISkill, IList<ISkillDay>>());
            }
            using (_mocks.Playback())
            {
                _target.Execute(scenario, period, skill, new ResourceCalculationDataContainerFromStorage());
            }
        }
    }
}