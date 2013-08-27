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
        private MockRepository _mocks;

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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
        	_lazyManager = _mocks.DynamicMock<ILazyLoadingManager>();
        	_skillDayLoadHelper = _mocks.DynamicMock<ISkillDayLoadHelper>();
        	_peopleLoader = _mocks.DynamicMock<IPeopleLoader>();
        	_peopleAndSkillLoaderDecider = _mocks.DynamicMock<IPeopleAndSkillLoaderDecider>();
        	_resourceOptimizationHelper = _mocks.DynamicMock<IResourceOptimizationHelper>();
            _loadScheduleByPersonSpecification = _mocks.Stub<LoadScheduleByPersonSpecification>();
            _uow = _mocks.DynamicMock<IUnitOfWork>();
            
            _permittedPeople = new List<IPerson> { _mocks.StrictMock<IPerson>() };
            _scenario = _mocks.StrictMock<IScenario>();

			_schedulerState = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_requestedPeriod, TimeZoneInfoFactory.UtcTimeZoneInfo()), _permittedPeople);

            target = new SchedulingResultLoader(_schedulerState, _repositoryFactory, _eventAggregator, _lazyManager, _peopleAndSkillLoaderDecider, _peopleLoader, _skillDayLoadHelper, _resourceOptimizationHelper, _loadScheduleByPersonSpecification);
        }

        [Test]
        public void VerifyOnEventForecastDataMessageHandler()
        {
            var skill = _mocks.StrictMock<ISkill>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();

			createSkillInitializeExpectation(skill);
			
            _mocks.ReplayAll();

            target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            target.ReloadForecastData(_uow);

            _mocks.VerifyAll();

            Assert.IsTrue(target.SchedulerState.SchedulingResultState.Skills.Contains(skill));
            Assert.AreSame(scheduleDictionary, target.SchedulerState.SchedulingResultState.Schedules);
        }

        [Test]
        public void VerifyReloadForecastDataWithoutSkills()
        {
            var skillRepository = _mocks.StrictMock<ISkillRepository>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();

            Expect.Call(_repositoryFactory.CreateSkillRepository(_uow)).Return(skillRepository).Repeat.Once();
            Expect.Call(skillRepository.FindAllWithSkillDays(_requestedPeriod)).Return(new List<ISkill>()).Repeat.Once();
            
            _mocks.ReplayAll();

            target.SchedulerState.SchedulingResultState.Schedules = scheduleDictionary;
            target.ReloadForecastData(_uow);

            _mocks.VerifyAll();

            Assert.AreSame(scheduleDictionary, target.SchedulerState.SchedulingResultState.Schedules);
        }

        [Test]
        public void VerifyOnEventScheduleDataMessageHandler()
        {
            var skill = _mocks.StrictMock<ISkill>();
            
            IScheduleDictionary scheduleDictionary = createScheduleInitializationExpectation();

            prepareStateHolderWithSkill(skill, new List<ISkillDay>());
            createReassociatePeopleExpectation();
            
            Expect.Call(_permittedPeople[0].PersonSchedulePeriods(new DateOnlyPeriod())).IgnoreArguments().Return(new List<ISchedulePeriod>());
            Expect.Call(_permittedPeople[0].Name).Return(new Name("first", "last")).Repeat.Any();
            Expect.Call(_permittedPeople[0].Id).Return(Guid.NewGuid()).Repeat.Any();
            Expect.Call(_permittedPeople[0].PermissionInformation).Return(new PermissionInformation(_permittedPeople[0]));
        	Expect.Call(skill.DefaultResolution).Return(15);

            _mocks.ReplayAll();

            target.ReloadScheduleData(_uow);

            _mocks.VerifyAll();

            Assert.AreSame(scheduleDictionary, target.SchedulerState.SchedulingResultState.Schedules);
        }

        private void createReassociatePeopleExpectation()
        {
            Expect.Call(()=>_uow.Reassociate(new List<IContract>())).IgnoreArguments();
            Expect.Call(()=>_uow.Reassociate(new List<IContractSchedule>())).IgnoreArguments();
            Expect.Call(()=>_uow.Reassociate(new List<IPerson>())).IgnoreArguments();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyLoadWithIntradayData()
        {
            var skill = _mocks.StrictMock<ISkill>();
            
            createContractScheduleInitializationExpectation();
            createContractLoadingExpectation();
            createMultiplicatorDefinitionSetLoadingExpectation();
            createSkillInitializeExpectation(skill);
            createScheduleInitializationExpectation();
            createInitializeCommonStateHolderExpectation();
			createPersonalAccountLoadingExpectation();
            createScheduleTagLoadingExpectation();

            Expect.Call(_permittedPeople[0].PersonSchedulePeriods(new DateOnlyPeriod())).IgnoreArguments().Return(new List<ISchedulePeriod>());
            Expect.Call(_permittedPeople[0].Name).Return(new Name("first", "last")).Repeat.Any();
            Expect.Call(_permittedPeople[0].Id).Return(Guid.NewGuid()).Repeat.Any();
            Expect.Call(_permittedPeople[0].EmploymentNumber).Return("hej");
            Expect.Call(_permittedPeople[0].PermissionInformation).Return(new PermissionInformation(_permittedPeople[0]));
        	
            _mocks.ReplayAll();
            target.LoadWithIntradayData(_uow);
            Assert.IsTrue(_schedulerState.FilteredPersonDictionary.Values.Contains(_permittedPeople[0]));
            _mocks.VerifyAll();
        }

        private void createInitializeCommonStateHolderExpectation()
        {
            var absenceRepository = _mocks.StrictMock<IAbsenceRepository>();
            Expect.Call(_repositoryFactory.CreateAbsenceRepository(_uow)).Return(absenceRepository);
            Expect.Call(absenceRepository.LoadAll()).Return(new List<IAbsence>());

            var activityRepository = _mocks.StrictMock<IActivityRepository>();
            Expect.Call(_repositoryFactory.CreateActivityRepository(_uow)).Return(activityRepository);
            Expect.Call(activityRepository.LoadAll()).Return(new List<IActivity>());

            var shiftCategoryRepository = _mocks.StrictMock<IShiftCategoryRepository>();
            Expect.Call(_repositoryFactory.CreateShiftCategoryRepository(_uow)).Return(shiftCategoryRepository);
            Expect.Call(shiftCategoryRepository.FindAll()).Return(new List<IShiftCategory>());

            var dayOffRepository = _mocks.StrictMock<IDayOffTemplateRepository>();
            Expect.Call(_repositoryFactory.CreateDayOffRepository(_uow)).Return(dayOffRepository);
            Expect.Call(dayOffRepository.LoadAll()).Return(new List<IDayOffTemplate>());
        }

        private void createContractScheduleInitializationExpectation()
        {
            var contractScheduleRepository = _mocks.StrictMock<IContractScheduleRepository>();

            Expect.Call(_repositoryFactory.CreateContractScheduleRepository(_uow))
                .Return(contractScheduleRepository)
                .Repeat.Twice();

            Expect.Call(contractScheduleRepository.LoadAllAggregate())
                .Return(new List<IContractSchedule>())
                .Repeat.Twice();
        }

        private void createContractLoadingExpectation()
        {
            var contractRepository = _mocks.StrictMock<IContractRepository>();

            Expect.Call(_repositoryFactory.CreateContractRepository(_uow))
                .Return(contractRepository)
                .Repeat.Twice();

            Expect.Call(contractRepository.FindAllContractByDescription())
                .Return(new List<IContract>())
                .Repeat.Twice();
        }

		private void createPersonalAccountLoadingExpectation()
		{
			var personAbsenceAccountRepository = _mocks.StrictMock<IPersonAbsenceAccountRepository>();

			Expect.Call(_repositoryFactory.CreatePersonAbsenceAccountRepository(_uow)).Return(personAbsenceAccountRepository).Repeat.Once();
			Expect.Call(personAbsenceAccountRepository.LoadAllAccounts()).Return(new Dictionary<IPerson, IPersonAccountCollection>()).Repeat.Once();
		}

        private void createMultiplicatorDefinitionSetLoadingExpectation()
        {
            var multiplicatorDefinitionSetRepository = _mocks.StrictMock<IMultiplicatorDefinitionSetRepository>();

            Expect.Call(_repositoryFactory.CreateMultiplicatorDefinitionSetRepository(_uow))
                .Return(multiplicatorDefinitionSetRepository)
                .Repeat.Once();

            Expect.Call(multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions())
                .Return(new List<IMultiplicatorDefinitionSet>())
                .Repeat.Once();
        }


        private void createSkillInitializeExpectation(ISkill skill)
        {
            var skillRepository = _mocks.StrictMock<ISkillRepository>();
            Expect.Call(_repositoryFactory.CreateSkillRepository(_uow))
                .Return(skillRepository)
                .Repeat.Once();

            Expect.Call(skillRepository.FindAllWithSkillDays(_requestedPeriod))
                .Return(new List<ISkill> { skill })
                .Repeat.Once();

            Expect.Call(skill.SkillType).Return(SkillTypeFactory.CreateSkillType());
        }

        private IScheduleDictionary createScheduleInitializationExpectation()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var scheduleRepository = _mocks.StrictMock<IScheduleRepository>();
        	var period = _schedulerState.RequestedPeriod.Period();

            Expect.Call(_repositoryFactory.CreateScheduleRepository(_uow))
                .Return(scheduleRepository);

        	Expect.Call(scheduleRepository.FindSchedulesForPersons(null, null, null, null, null)).Constraints(
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

        private void prepareStateHolderWithSkill(ISkill skill, IList<ISkillDay> skillDays)
        {
            _schedulerState.SchedulingResultState.Skills.Add(skill);
            _schedulerState.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>{{skill, skillDays}};
        }

        private void createScheduleTagLoadingExpectation()
        {
            var scheduleTagRep = _mocks.StrictMock<IScheduleTagRepository>();

            Expect.Call(_repositoryFactory.CreateScheduleTagRepository(_uow)).Return(scheduleTagRep).Repeat.Once();
            Expect.Call(scheduleTagRep.LoadAll()).Return( new List<IScheduleTag>()).Repeat.Once();
        }
    }
}
