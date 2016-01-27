using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimization
	{
		OptimizationResultModel Optimize(Guid planningPeriodId);
	}

	public class IntradayOptimization : IIntradayOptimization
	{
		private readonly IDailyValueByAllSkillsExtractor _dailyValueByAllSkillsExtractor;
		private readonly IIntradayDecisionMaker _intradayDecisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
		private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IMinWeekWorkTimeRule _minWeekWorkTimeRule;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly SetupStateHolderForWebScheduling _setupStateHolderForWebScheduling;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IScheduleControllerPrerequisites _prerequisites;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;

		public IntradayOptimization(IDailyValueByAllSkillsExtractor dailyValueByAllSkillsExtractor, 
									IIntradayDecisionMaker intradayDecisionMaker, IScheduleService scheduleService,
									OptimizationPreferencesFactory optimizationPreferencesFactory,
									Func<ISchedulerStateHolder> schedulerStateHolder,
									Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
									ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
									ISkillIntervalDataDivider skillIntervalDataDivider,
									ISkillIntervalDataAggregator skillIntervalDataAggregator,
									IEffectiveRestrictionCreator effectiveRestrictionCreator,
									IMinWeekWorkTimeRule minWeekWorkTimeRule,
									IResourceOptimizationHelper resourceOptimizationHelper,
									DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
									IFixedStaffLoader fixedStaffLoader,
									SetupStateHolderForWebScheduling setupStateHolderForWebScheduling,
									Func<IPersonSkillProvider> personSkillProvider,
									IMatrixListFactory matrixListFactory,
									IScheduleDayEquator scheduleDayEquator,
									IPlanningPeriodRepository planningPeriodRepository,
									IScheduleControllerPrerequisites prerequisites,
									Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
									IScheduleDictionaryPersister persister,
									IOptimizerHelperHelper optimizerHelperHelper
									)
		{
			_dailyValueByAllSkillsExtractor = dailyValueByAllSkillsExtractor;
			_intradayDecisionMaker = intradayDecisionMaker;
			_scheduleService = scheduleService;
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataDivider = skillIntervalDataDivider;
			_skillIntervalDataAggregator = skillIntervalDataAggregator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_minWeekWorkTimeRule = minWeekWorkTimeRule;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_fixedStaffLoader = fixedStaffLoader;
			_setupStateHolderForWebScheduling = setupStateHolderForWebScheduling;
			_personSkillProvider = personSkillProvider;
			_matrixListFactory = matrixListFactory;
			_scheduleDayEquator = scheduleDayEquator;
			_planningPeriodRepository = planningPeriodRepository;
			_prerequisites = prerequisites;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_persister = persister;
			_optimizerHelperHelper = optimizerHelperHelper;
		}

		public virtual OptimizationResultModel Optimize(Guid planningPeriodId)
		{
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffOptimizationPreference = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			_prerequisites.MakeSureLoaded();
			var people = _fixedStaffLoader.Load(period);
			_setupStateHolderForWebScheduling.Setup(period, people);

			var allSchedules = extractAllSchedules(_schedulerStateHolder().SchedulingResultState, people, period);
			initializePersonSkillProviderBeforeAccessingItFromOtherThreads(period, people.AllPeople);

			var matrixListForIntraDayOptimizationOriginal = _matrixListFactory.CreateMatrixListForSelection(allSchedules);
			var matrixOriginalStateContainerListForIntradayOptimizationOriginal =
				matrixListForIntraDayOptimizationOriginal.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();

			var matrixListForIntraDayOptimizationWork = _matrixListFactory.CreateMatrixListForSelection(allSchedules);
			var matrixOriginalStateContainerListForIntradayOptimizationWork =
				matrixListForIntraDayOptimizationWork.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();

			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(
					_schedulerStateHolder().SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag));
			
			var creator = new IntradayOptimizer2Creator(
				matrixOriginalStateContainerListForIntradayOptimizationOriginal,
				matrixOriginalStateContainerListForIntradayOptimizationWork,
				_intradayDecisionMaker,
				_scheduleService,
				optimizationPreferences,
				rollbackService,
				_schedulerStateHolder().SchedulingResultState,
				_skillStaffPeriodToSkillIntervalDataMapper,
				_skillIntervalDataDivider,
				_skillIntervalDataAggregator,
				_effectiveRestrictionCreator,
				_minWeekWorkTimeRule,
				_resourceOptimizationHelper,
				dayOffOptimizationPreference);

			var optimizers = creator.Create();	
			var service = new IntradayOptimizerContainer(_dailyValueByAllSkillsExtractor);
			var minutesPerInterval = 15;

			if (_schedulerStateHolder().SchedulingResultState.Skills.Any())
			{
				minutesPerInterval = _schedulerStateHolder().SchedulingResultState.Skills.Min(s => s.DefaultResolution);
			}

			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(_schedulerStateHolder().Schedules);

			_optimizerHelperHelper.LockDaysForIntradayOptimization(matrixListForIntraDayOptimizationOriginal, period);

			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(new NoBackgroundWorker());
				service.Execute(optimizers, period, optimizationPreferences.Advanced.TargetValueCalculation);
			}

			_persister.Persist(_schedulerStateHolder().Schedules);

			var result = new OptimizationResultModel();
			result.Map(_schedulerStateHolder().SchedulingResultState.SkillDays, planningPeriod.Range);
			return result;
		}

		private static IList<IScheduleDay> extractAllSchedules(ISchedulingResultStateHolder stateHolder, PeopleSelection people, DateOnlyPeriod period)
		{
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules)
			{
				if (people.FixedStaffPeople.Contains(schedule.Key))
				{
					allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
				}
			}
			return allSchedules;
		}

		private void initializePersonSkillProviderBeforeAccessingItFromOtherThreads(DateOnlyPeriod period, IEnumerable<IPerson> allPeople)
		{
			var provider = _personSkillProvider();
			var dayCollection = period.DayCollection();
			allPeople.ForEach(p => dayCollection.ForEach(d => provider.SkillsOnPersonDate(p, d)));
		}
	}
}