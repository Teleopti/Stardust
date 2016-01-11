using System.Collections.ObjectModel;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Rhino.Mocks;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class SchedulerStateLoaderTest
    {
        private SchedulerStateLoader _targetStateLoader;
        private ISchedulerStateHolder _targetStateHolder;
        private IScenario _targetScenario;
        private readonly DateOnlyPeriod _targetPeriod = new DateOnlyPeriod(2008, 10, 20, 2008, 10, 21);
        private IList<IPerson> _permittedPeople;
        private ISkill _selectedSkill;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
    	private ILazyLoadingManager _lazyManager;
	    private DateTimePeriod _period;

	    [SetUp]
        public void Setup()
        {
            _permittedPeople = new List<IPerson> { PersonFactory.CreatePerson() };

			_period = _targetPeriod.ToDateTimePeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
            _unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            _repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
            _targetScenario = ScenarioFactory.CreateScenarioAggregate();
            _selectedSkill = SkillFactory.CreateSkill("Phone");
			_targetStateHolder = new SchedulerStateHolder(_targetScenario, new DateOnlyPeriodAsDateTimePeriod(_targetPeriod, TimeZoneInfoFactory.UtcTimeZoneInfo()), _permittedPeople, MockRepository.GenerateMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder(), new TimeZoneGuardWrapper());
        	_lazyManager = MockRepository.GenerateMock<ILazyLoadingManager>();
        }

        [Test]
        public void VerifyInstanceIsCreatedWithoutRepositoryFactory()
        {
            _targetStateLoader = new SchedulerStateLoader(_targetStateHolder);
            Assert.IsNotNull(_targetStateLoader);
        }

        [Test]
        public void VerifyInstanceIsCreatedWithRepositoryFactory()
        {
			_targetStateLoader = new SchedulerStateLoader(_targetStateHolder, _repositoryFactory, _unitOfWorkFactory, _lazyManager);
            Assert.IsNotNull(_targetStateLoader);
        }

        [Test]
        public void VerifyLoadSchedulingResult()
        {
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
            var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
            var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
            var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
            
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
            _repositoryFactory.Stub(x => x.CreateActivityRepository(uow)).Return(activityRepository);
            _repositoryFactory.Stub(x => x.CreateScheduleRepository(uow)).Return(scheduleRepository);
            _repositoryFactory.Stub(x => x.CreateSkillRepository(uow)).Return(skillRepository);
            _repositoryFactory.Stub(x => x.CreateMultisiteDayRepository(uow)).Return(MockRepository.GenerateMock<IMultisiteDayRepository>());
            _repositoryFactory.Stub(x => x.CreateSkillDayRepository(uow)).Return(skillDayRepository);
            skillRepository.Stub(x => x.FindAllWithSkillDays(_targetPeriod)).Return(new Collection<ISkill>{_selectedSkill});
            activityRepository.Stub(x => x.LoadAll()).Return(new List<IActivity>());
            skillDayRepository.Stub(x => x.FindReadOnlyRange(_targetPeriod, new List<ISkill>(), _targetScenario)).IgnoreArguments().Return(new Collection<ISkillDay>());
            scheduleDictionary.Stub(x => x.Keys).Return(new Collection<IPerson>());

            _targetStateLoader = new SchedulerStateLoader(_targetStateHolder, _repositoryFactory, _unitOfWorkFactory, _lazyManager);
            var scheduleDateTimePeriod =
                new ScheduleDateTimePeriod(_period);
            _targetStateLoader.LoadSchedules(scheduleDateTimePeriod);
            _targetStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
            _targetStateLoader.LoadSchedulingResultAsync(scheduleDateTimePeriod, uow, new BackgroundWorker(), new List<ISkill> { _selectedSkill });

            Assert.IsTrue(_targetStateHolder.SchedulingResultState.Skills.Contains(_selectedSkill));
            Assert.IsTrue(_targetStateHolder.AllPermittedPersons.Contains(_permittedPeople[0]));
            Assert.AreSame(scheduleDictionary, _targetStateHolder.SchedulingResultState.Schedules);
        }
    }
}
