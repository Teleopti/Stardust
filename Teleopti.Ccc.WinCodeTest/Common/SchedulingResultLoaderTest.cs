using System;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Common;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class SchedulingResultLoaderTest
    {
        private readonly DateOnlyPeriod _requestedPeriod = new DateOnlyPeriod(2008, 10, 20, 2008,10,20);
        private IList<IPerson> _permittedPeople;
        private IScenario _scenario;
        private ISchedulerStateHolder _schedulerState;
        private IRepositoryFactory _repositoryFactory;
    	private ILazyLoadingManager _lazyManager;
        private IUnitOfWork _uow;
        private readonly IEventAggregator _eventAggregator=new EventAggregator();
        private ISkillDayLoadHelper _skillDayLoadHelper;
        private IPeopleLoader _peopleLoader;
        private IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private ISchedulingResultLoader target;
        private LoadScheduleByPersonSpecification _loadScheduleByPersonSpecification;
	    private ISkill _skill;

	    [SetUp]
        public void Setup()
        {
            _repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
        	_lazyManager = MockRepository.GenerateMock<ILazyLoadingManager>();
        	_skillDayLoadHelper = MockRepository.GenerateMock<ISkillDayLoadHelper>();
        	_peopleLoader = MockRepository.GenerateMock<IPeopleLoader>();
        	_peopleAndSkillLoaderDecider = MockRepository.GenerateMock<IPeopleAndSkillLoaderDecider>();
        	_resourceOptimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
            _loadScheduleByPersonSpecification = MockRepository.GenerateMock<LoadScheduleByPersonSpecification>();
            _uow = MockRepository.GenerateMock<IUnitOfWork>();
            
            _permittedPeople = new List<IPerson> { MockRepository.GenerateMock<IPerson>() };
            _scenario = MockRepository.GenerateMock<IScenario>();
		    _skill = SkillFactory.CreateSkill("Phone");

			_schedulerState = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_requestedPeriod, TimeZoneInfoFactory.UtcTimeZoneInfo()), _permittedPeople);

            target = new SchedulingResultLoader(_schedulerState, _repositoryFactory, _eventAggregator, _lazyManager, _peopleAndSkillLoaderDecider, _peopleLoader, _skillDayLoadHelper, _resourceOptimizationHelper, _loadScheduleByPersonSpecification);
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandler()
        {
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			
			createSkillInitializeExpectation();
            
			target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            target.ReloadForecastData(_uow);

            Assert.IsTrue(target.SchedulerState.SchedulingResultState.Skills.Contains(_skill));
            Assert.AreSame(scheduleDictionary, target.SchedulerState.SchedulingResultState.Schedules);
        }

        [Test]
        public void VerifyReloadForecastDataWithoutSkills()
        {
            var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();

            _repositoryFactory.Stub(x => x.CreateSkillRepository(_uow)).Return(skillRepository);
            skillRepository.Stub(x=>x.FindAllWithSkillDays(_requestedPeriod)).Return(new List<ISkill>());
            
            target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            target.ReloadForecastData(_uow);

            Assert.AreSame(scheduleDictionary, target.SchedulerState.SchedulingResultState.Schedules);
        }

        [Test]
        public void VerifyOnEventScheduleDataMessageHandler()
        {
            IScheduleDictionary scheduleDictionary = createScheduleInitializationExpectation();

            _permittedPeople[0].Stub(x => x.PersonSchedulePeriods(new DateOnlyPeriod())).IgnoreArguments().Return(new List<ISchedulePeriod>());
            _permittedPeople[0].Stub(x =>x.Name).Return(new Name("first", "last"));
            _permittedPeople[0].Stub(x =>x.Id).Return(Guid.NewGuid());
			_permittedPeople[0].Stub(x => x.PermissionInformation).Return(new PermissionInformation(_permittedPeople[0]));

            target.ReloadScheduleData(_uow);

            Assert.AreSame(scheduleDictionary, target.SchedulerState.SchedulingResultState.Schedules);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyLoadWithIntradayData()
        {
            createContractScheduleInitializationExpectation();
            createContractLoadingExpectation();
            createMultiplicatorDefinitionSetLoadingExpectation();
            createSkillInitializeExpectation();
            createScheduleInitializationExpectation();
            createInitializeCommonStateHolderExpectation();
			createPersonalAccountLoadingExpectation();
            createScheduleTagLoadingExpectation();
			
			_permittedPeople[0].Stub(x => x.PersonSchedulePeriods(new DateOnlyPeriod())).IgnoreArguments().Return(new List<ISchedulePeriod>());
			_permittedPeople[0].Stub(x => x.Name).Return(new Name("first", "last"));
			_permittedPeople[0].Stub(x => x.EmploymentNumber).Return("hej");
			_permittedPeople[0].Stub(x => x.Id).Return(Guid.NewGuid());
			_permittedPeople[0].Stub(x => x.PermissionInformation).Return(new PermissionInformation(_permittedPeople[0]));

            target.LoadWithIntradayData(_uow);
            Assert.IsTrue(_schedulerState.FilteredPersonDictionary.Values.Contains(_permittedPeople[0]));
        }

        private void createInitializeCommonStateHolderExpectation()
        {
            var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
            _repositoryFactory.Stub(x=>x.CreateAbsenceRepository(_uow)).Return(absenceRepository);
            absenceRepository.Stub(x=>x.LoadAll()).Return(new List<IAbsence>());

            var activityRepository = MockRepository.GenerateMock<IActivityRepository>();
            _repositoryFactory.Stub(x=>x.CreateActivityRepository(_uow)).Return(activityRepository);
            activityRepository.Stub(x=>x.LoadAll()).Return(new List<IActivity>());

            var shiftCategoryRepository = MockRepository.GenerateMock<IShiftCategoryRepository>();
            _repositoryFactory.Stub(x=>x.CreateShiftCategoryRepository(_uow)).Return(shiftCategoryRepository);
            shiftCategoryRepository.Stub(x=>x.FindAll()).Return(new List<IShiftCategory>());

            var dayOffRepository = MockRepository.GenerateMock<IDayOffTemplateRepository>();
            _repositoryFactory.Stub(x=>x.CreateDayOffRepository(_uow)).Return(dayOffRepository);
            dayOffRepository.Stub(x=>x.LoadAll()).Return(new List<IDayOffTemplate>());
        }

        private void createContractScheduleInitializationExpectation()
        {
            var contractScheduleRepository = MockRepository.GenerateMock<IContractScheduleRepository>();

            _repositoryFactory.Stub(x => x.CreateContractScheduleRepository(_uow)).Return(contractScheduleRepository);
            contractScheduleRepository.Stub(x => x.LoadAllAggregate()).Return(new List<IContractSchedule>());
        }

        private void createContractLoadingExpectation()
        {
            var contractRepository = MockRepository.GenerateMock<IContractRepository>();

            _repositoryFactory.Stub(x => x.CreateContractRepository(_uow)).Return(contractRepository);
            contractRepository.Stub(x => x.FindAllContractByDescription()).Return(new List<IContract>());
        }

		private void createPersonalAccountLoadingExpectation()
		{
			var personAbsenceAccountRepository = MockRepository.GenerateMock<IPersonAbsenceAccountRepository>();

			_repositoryFactory.Stub(x => x.CreatePersonAbsenceAccountRepository(_uow)).Return(personAbsenceAccountRepository);
			personAbsenceAccountRepository.Stub(x=>x.LoadAllAccounts()).Return(new Dictionary<IPerson, IPersonAccountCollection>());
		}

        private void createMultiplicatorDefinitionSetLoadingExpectation()
        {
            var multiplicatorDefinitionSetRepository = MockRepository.GenerateMock<IMultiplicatorDefinitionSetRepository>();

            _repositoryFactory.Stub(x=>x.CreateMultiplicatorDefinitionSetRepository(_uow)).Return(multiplicatorDefinitionSetRepository);
            multiplicatorDefinitionSetRepository.Stub(x => x.FindAllOvertimeDefinitions()).Return(new List<IMultiplicatorDefinitionSet>());
        }

        private void createSkillInitializeExpectation()
        {
            var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
            
			_repositoryFactory.Stub(x=>x.CreateSkillRepository(_uow)).Return(skillRepository);
	        skillRepository.Stub(x => x.FindAllWithSkillDays(_requestedPeriod)).Return(new List<ISkill> {_skill});
        }

        private IScheduleDictionary createScheduleInitializationExpectation()
        {
            var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
            var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
        	var period = _schedulerState.RequestedPeriod.Period();

            _repositoryFactory.Stub(x => x.CreateScheduleRepository(_uow)).Return(scheduleRepository);
        	scheduleRepository.Stub(x=>x.FindSchedulesForPersons(null, null, null, null, null)).Constraints(
        		Rhino.Mocks.Constraints.Is.Matching(
        			new Predicate<IScheduleDateTimePeriod>(
        				x =>
        				x.VisiblePeriod.StartDateTime == period.StartDateTime.AddHours(-24) &&
        				x.VisiblePeriod.EndDateTime == period.EndDateTime.AddHours(24))),
        		Rhino.Mocks.Constraints.Is.Equal(_scenario), Rhino.Mocks.Constraints.Is.Anything(),
        		Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Equal(_permittedPeople)).Return(
        			scheduleDictionary);

            return scheduleDictionary;
        }

        private void createScheduleTagLoadingExpectation()
        {
            var scheduleTagRep = MockRepository.GenerateMock<IScheduleTagRepository>();

            _repositoryFactory.Stub(x => x.CreateScheduleTagRepository(_uow)).Return(scheduleTagRep);
            scheduleTagRep.Stub(x => x.LoadAll()).Return( new List<IScheduleTag>());
        }
    }
}
