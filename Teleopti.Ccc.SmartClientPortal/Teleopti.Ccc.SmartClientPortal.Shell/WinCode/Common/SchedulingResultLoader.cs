using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class SchedulingResultLoader : ISchedulingResultLoader
    {
        private readonly IRepositoryFactory _repositoryFactory;

        private readonly IEventAggregator _eventAggregator;
        private readonly ILazyLoadingManager _lazyManager;
        private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
        private readonly IPeopleLoader _peopleLoader;
        private readonly ISkillDayLoadHelper _skillDayLoadHelper;
        private readonly IResourceCalculation _resourceOptimizationHelper;
	    private readonly IScheduleStorageFactory _scheduleStorageFactory;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private ILoaderDeciderResult _deciderResult;

	    public ISchedulerStateHolder SchedulerState { get; private set; }

        public IEnumerable<IContractSchedule> ContractSchedules { get; private set; }

        public IEnumerable<IContract> Contracts { get; private set; }

        public IEnumerable<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSets { get; private set; }

        public SchedulingResultLoader(ISchedulerStateHolder stateHolder,
                                    IRepositoryFactory repositoryFactory,
                                    IEventAggregator eventAggregator,
                                    ILazyLoadingManager lazyManager,
                                    IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider,
                                    IPeopleLoader peopleLoader,
            ISkillDayLoadHelper skillDayLoadHelper,
            IResourceCalculation resourceOptimizationHelper,
						IScheduleStorageFactory scheduleStorageFactory,
			CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory)
        {
            SchedulerState = stateHolder;

            _repositoryFactory = repositoryFactory;
            _eventAggregator = eventAggregator;
            _lazyManager = lazyManager;
            _peopleAndSkillLoaderDecider = peopleAndSkillLoaderDecider;
            _peopleLoader = peopleLoader;
            _skillDayLoadHelper = skillDayLoadHelper;
            _resourceOptimizationHelper = resourceOptimizationHelper;
	        _scheduleStorageFactory = scheduleStorageFactory;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
		}

        public void LoadWithIntradayData(IUnitOfWork unitOfWork)
        {
            loadCommonState(unitOfWork);
            loadContractSchedules(unitOfWork);
            loadContracts(unitOfWork);
            loadDefinitionSets(unitOfWork);
            loadPersonalAccounts(unitOfWork);
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(UserTexts.Resources.LoadingPeopleTreeDots);
            initializeSkills(unitOfWork);
			_peopleLoader.Initialize(SchedulerState); 
			initializeDecider();
            filterSkills();
            initializePeopleInOrganization();
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(UserTexts.Resources.LoadingSkillDataTreeDots);
            initializeSkillDays();
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(UserTexts.Resources.LoadingSchedulesTreeDots);
            initializeSchedules(unitOfWork);
            _eventAggregator.GetEvent<IntradayLoadProgress>().Publish(
                UserTexts.Resources.InitializingScheduleDataThreeDots);
            InitializeScheduleData();
        }

        private void loadPersonalAccounts(IUnitOfWork uow)
        {
            var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(uow);
            SchedulerState.SchedulingResultState.AllPersonAccounts = personAbsenceAccountRepository.LoadAllAccounts();
        }

        private void loadDefinitionSets(IUnitOfWork uow)
        {
            IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository = new MultiplicatorDefinitionSetRepository(uow);
            MultiplicatorDefinitionSets = multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions();
        }

        public void ReloadForecastData(IUnitOfWork unitOfWork)
        {
            initializeSkills(unitOfWork);
			makeSureDeciderIsInitializedForCrappyTests();
            filterSkills();
            if (!SchedulerState.SchedulingResultState.Skills.IsEmpty())
            {
                initializeSkillDays();
                InitializeScheduleData();
            }
        }

	    private void makeSureDeciderIsInitializedForCrappyTests()
	    {
		    if (_deciderResult == null) initializeDecider();
	    }

	    public void ReloadScheduleData(IUnitOfWork unitOfWork)
        {
            unitOfWork.Reassociate(SchedulerState.CommonStateHolder.ShiftCategories);
            unitOfWork.Reassociate(SchedulerState.CommonStateHolder.Absences);
            unitOfWork.Reassociate(SchedulerState.CommonStateHolder.Activities);
            unitOfWork.Reassociate(SchedulerState.CommonStateHolder.DayOffs);
            reassociatePeople(unitOfWork);
            initializeSchedules(unitOfWork);
            InitializeScheduleData();
        }

        private void filterSkills()
        {
            _deciderResult.FilterSkills(SchedulerState.SchedulingResultState.Skills, SchedulerState.SchedulingResultState.RemoveSkill,s => SchedulerState.SchedulingResultState.AddSkills(s));
        }

	    private void initializeSkills(IUnitOfWork uow)
	    {
		    var skills = _repositoryFactory
			    .CreateSkillRepository(uow)
			    .FindAllWithSkillDays(SchedulerState.RequestedPeriod.DateOnlyPeriod)
			    .ForEach(s =>
			    {
					_lazyManager.Initialize(s);
					_lazyManager.Initialize(s.SkillType);
			    })
			    .ToArray();

		    SchedulerState.SchedulingResultState.ClearSkills();
		    SchedulerState.SchedulingResultState.AddSkills(skills);
	    }

	    private void initializeSkillDays()
        {
            SchedulerState.SchedulingResultState.SkillDays =
                _skillDayLoadHelper.LoadSchedulerSkillDays(SchedulerState.RequestedPeriod.DateOnlyPeriod,
                                                           SchedulerState.SchedulingResultState.Skills,
                                                           SchedulerState.RequestedScenario);
        }

        private void initializeDecider()
        {
	        _deciderResult = _peopleAndSkillLoaderDecider.Execute(SchedulerState.RequestedScenario, SchedulerState.RequestedPeriod.Period(),
		        SchedulerState.ChoosenAgents);
        }

        private void initializePeopleInOrganization()
        {
            ICollection<IPerson> peopleInOrg = SchedulerState.SchedulingResultState.LoadedAgents;
            _deciderResult.FilterPeople(peopleInOrg);

            peopleInOrg = new HashSet<IPerson>(peopleInOrg);
            SchedulerState.ChoosenAgents.ForEach(peopleInOrg.Add);
            SchedulerState.SchedulingResultState.LoadedAgents = peopleInOrg;

            SchedulerState.ResetFilteredPersons();
        }

        private void initializeSchedules(IUnitOfWork uow)
        {
            var scheduleStorage = _scheduleStorageFactory.Create(uow);

			var requestedPeriod = SchedulerState.RequestedPeriod.Period().ChangeEndTime(TimeSpan.FromHours(24)).ChangeStartTime(TimeSpan.FromHours(-24));
            var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, true);

            SchedulerState.LoadSchedules(scheduleStorage, SchedulerState.SchedulingResultState.LoadedAgents, scheduleDictionaryLoadOptions, requestedPeriod);
        }

        public void InitializeScheduleData()
        {
			if (SchedulerState.SchedulingResultState.Skills.IsEmpty()) return;

        	splitAllWorkloadDaysWithMergedIntervals();
            var dateOnlyPeriod =
                SchedulerState.RequestedPeriod.DateOnlyPeriod;
			using (_cascadingResourceCalculationContextFactory.Create(SchedulerState.SchedulingResultState, false, dateOnlyPeriod))
			{
				foreach (var dateOnly in dateOnlyPeriod.DayCollection())
				{
					_resourceOptimizationHelper.ResourceCalculate(dateOnly, SchedulerState.SchedulingResultState.ToResourceOptimizationData(true, true));
				}				
			}
        }

    	private void splitAllWorkloadDaysWithMergedIntervals()
    	{
    	    if (SchedulerState == null || SchedulerState.SchedulingResultState == null ||
    	        SchedulerState.SchedulingResultState.SkillDays == null)
    	        return;

    	    foreach (var skillDayItem in SchedulerState.SchedulingResultState.SkillDays)
			{
				var resolution = TimeSpan.FromMinutes(skillDayItem.Key.DefaultResolution);
				var workloadDays = skillDayItem.Value.SelectMany(s => s.WorkloadDayCollection);
				foreach (var workloadDay in workloadDays)
				{
					var mergedIntervals = workloadDay.TaskPeriodList.Where(t => t.Period.ElapsedTime() > resolution).ToList();
					workloadDay.SplitTemplateTaskPeriods(mergedIntervals);
				}
			}
    	}

    	private void reassociatePeople(IUnitOfWork uow)
        {
            uow.Reassociate(Contracts);
            uow.Reassociate(ContractSchedules);
            uow.Reassociate(SchedulerState.SchedulingResultState.LoadedAgents);
        }

        private void loadCommonState(IUnitOfWork uow)
        {
            SchedulerState.LoadCommonState(uow, _repositoryFactory);
        }

        private void loadContractSchedules(IUnitOfWork uow)
        {
            using (uow.DisableFilter(QueryFilter.Deleted))
            {
                IEnumerable<IContractSchedule> list = _repositoryFactory
                    .CreateContractScheduleRepository(uow)
                    .LoadAllAggregate();

                ContractSchedules = new List<IContractSchedule>(list);
            }
        }

        private void loadContracts(IUnitOfWork uow)
        {
            using (uow.DisableFilter(QueryFilter.Deleted))
            {
                IEnumerable<IContract> list = _repositoryFactory
                    .CreateContractRepository(uow)
                    .FindAllContractByDescription();

                Contracts = new List<IContract>(list);
            }
        }
    }
}
