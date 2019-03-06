using System.Collections.ObjectModel;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;

using NUnit.Framework;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class SchedulerStateLoaderTest
    {
        private SchedulerStateLoader _targetStateLoader;
        private SchedulingScreenState _targetStateHolder;
        private IScenario _targetScenario;
        private readonly DateOnlyPeriod _targetPeriod = new DateOnlyPeriod(2008, 10, 20, 2008, 10, 21);
        private IList<IPerson> _permittedPeople;
        private ISkill _selectedSkill;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
	    private IScheduleStorageFactory _scheduleStorageFactory;
    	private ILazyLoadingManager _lazyManager;
	    private DateTimePeriod _period;
		
		[SetUp]
        public void Setup()
        {
            _permittedPeople = new List<IPerson> { PersonFactory.CreatePerson() };
			_period = _targetPeriod.ToDateTimePeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
            _unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            _repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
	        _scheduleStorageFactory = MockRepository.GenerateMock<IScheduleStorageFactory>();
			_targetScenario = ScenarioFactory.CreateScenarioAggregate();
			var schedules = new ScheduleDictionary(_targetScenario, new ScheduleDateTimePeriod(_period), null, null);
			var schedulingResultStateHolder = new SchedulingResultStateHolder() {Schedules = schedules};
			_selectedSkill = SkillFactory.CreateSkill("Phone");
			_targetStateHolder = new SchedulingScreenState(null, new SchedulerStateHolder(_targetScenario, new DateOnlyPeriodAsDateTimePeriod(_targetPeriod, TimeZoneInfoFactory.UtcTimeZoneInfo()), _permittedPeople, MockRepository.GenerateMock<IDisableDeletedFilter>(), schedulingResultStateHolder));
        	_lazyManager = MockRepository.GenerateMock<ILazyLoadingManager>();
        }
		
        [Test]
        public void VerifyInstanceIsCreatedWithRepositoryFactory()
        {
			_targetStateLoader = new SchedulerStateLoader(_targetStateHolder, _repositoryFactory, _unitOfWorkFactory, _lazyManager, _scheduleStorageFactory, new FakeTimeZoneGuard());
            Assert.IsNotNull(_targetStateLoader);
        }

        [Test]
        public void VerifyLoadSchedulingResult()
        {
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
            var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
            var scheduleDictionary = new ScheduleDictionaryForTest(ScenarioFactory.CreateScenario("default",true,false),_period);
            var scheduleRepository = MockRepository.GenerateMock<IScheduleStorage, IFindSchedulesForPersons>();
            var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
            var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();

			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
            _repositoryFactory.Stub(x => x.CreateActivityRepository(uow)).Return(activityRepository);
			_scheduleStorageFactory.Stub(x => x.Create(uow)).Return(scheduleRepository);
            _repositoryFactory.Stub(x => x.CreateSkillRepository(uow)).Return(skillRepository);
            _repositoryFactory.Stub(x => x.CreateMultisiteDayRepository(uow)).Return(MockRepository.GenerateMock<IMultisiteDayRepository>());
            _repositoryFactory.Stub(x => x.CreateSkillDayRepository(uow)).Return(skillDayRepository);
            skillRepository.Stub(x => x.FindAllWithSkillDays(_targetPeriod)).Return(new Collection<ISkill>{_selectedSkill});
            activityRepository.Stub(x => x.LoadAll()).Return(new List<IActivity>());
            skillDayRepository.Stub(x => x.FindReadOnlyRange(_targetPeriod, new List<ISkill>(), _targetScenario)).IgnoreArguments().Return(new Collection<ISkillDay>());
			

            _targetStateLoader = new SchedulerStateLoader(_targetStateHolder, _repositoryFactory, _unitOfWorkFactory, _lazyManager, _scheduleStorageFactory, new FakeTimeZoneGuard());
            var scheduleDateTimePeriod = new ScheduleDateTimePeriod(_period);
            _targetStateLoader.LoadSchedules(scheduleDateTimePeriod);
            _targetStateHolder.SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;
			

			_targetStateLoader.LoadSchedulingResultAsync(scheduleDateTimePeriod, new BackgroundWorker(), new List<ISkill> { _selectedSkill }, new StaffingCalculatorServiceFacade());

            Assert.IsTrue(_targetStateHolder.SchedulerStateHolder.SchedulingResultState.Skills.Contains(_selectedSkill));
            Assert.IsTrue(_targetStateHolder.SchedulerStateHolder.ChoosenAgents.Contains(_permittedPeople[0]));
            Assert.AreSame(scheduleDictionary, _targetStateHolder.SchedulerStateHolder.SchedulingResultState.Schedules);
        }
    }
}
