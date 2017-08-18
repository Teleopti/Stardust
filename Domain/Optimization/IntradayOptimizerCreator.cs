using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class IntradayOptimizerCreator
	{
		private readonly IntradayDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
		private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IIntradayOptimizeOneDayCallback _intradayOptimizeOneDayCallback;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly PersonalSkillsProvider _personalSkillsProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

		public IntradayOptimizerCreator(
			IntradayDecisionMaker decisionMaker,
			IScheduleService scheduleService,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataDivider skillIntervalDataDivider,
			ISkillIntervalDataAggregator skillIntervalDataAggregator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IResourceCalculation resourceOptimizationHelper,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IIntradayOptimizeOneDayCallback intradayOptimizeOneDayCallback,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			IScheduleDayEquator scheduleDayEquator,
			IOptimizerHelperHelper optimizerHelperHelper,
			MatrixListFactory matrixListFactory,
			PersonalSkillsProvider personalSkillsProvider,
			IUserTimeZone userTimeZone,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter)
		{
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataDivider = skillIntervalDataDivider;
			_skillIntervalDataAggregator = skillIntervalDataAggregator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_intradayOptimizeOneDayCallback = intradayOptimizeOneDayCallback;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleDayEquator = scheduleDayEquator;
			_optimizerHelperHelper = optimizerHelperHelper;
			_matrixListFactory = matrixListFactory;
			_personalSkillsProvider = personalSkillsProvider;
			_userTimeZone = userTimeZone;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
		}

		public IEnumerable<IIntradayOptimizer2> Create(DateOnlyPeriod period, IEnumerable<IPerson> agents, IOptimizationPreferences optimizerPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var scheduleMatrixes = _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, agents, period);
			var scheduleMatrixContainerList = scheduleMatrixes.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator)).ToList();
			var matrixes = scheduleMatrixContainerList.Select(container => container.ScheduleMatrix);
			_optimizerHelperHelper.LockDaysForIntradayOptimization(matrixes, period);

			var rollbackService = new SchedulePartModifyAndRollbackService(
					_schedulerStateHolder().SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(optimizerPreferences.General.ScheduleTag));

			IList<IIntradayOptimizer2> result = new List<IIntradayOptimizer2>();
			var workShiftContainerList = scheduleMatrixContainerList.ToList();

			for (int index = 0; index < scheduleMatrixContainerList.Count; index++)
			{
				var originalStateContainer = scheduleMatrixContainerList[index];

				var scheduleMatrix = originalStateContainer.ScheduleMatrix;

				var personalSkillsDataExtractor = new RelativeDailyValueByPersonalSkillsExtractor(scheduleMatrix, optimizerPreferences.Advanced, _skillStaffPeriodToSkillIntervalDataMapper,
				    _skillIntervalDataDivider, _skillIntervalDataAggregator, _personalSkillsProvider, _schedulerStateHolder().SchedulingResultState, _userTimeZone);
				
				var workShiftStateContainer = workShiftContainerList[index];

				var restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrix.Person, scheduleMatrix.EffectivePeriodDays.First().Day);

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateContainer, dayOffOptimizationPreference);

				var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();

				var optimizer = new IntradayOptimizer2(personalSkillsDataExtractor, _decisionMaker, scheduleMatrix,
					new IntradayOptimizeOneday(_scheduleService, optimizerPreferences, rollbackService,
						_resourceOptimizationHelper,
						_effectiveRestrictionCreator, optimizationLimits, workShiftStateContainer, schedulingOptionsCreator,
						_mainShiftOptimizeActivitySpecificationSetter, _deleteAndResourceCalculateService,
						scheduleMatrix, _intradayOptimizeOneDayCallback, _schedulerStateHolder().SchedulingResultState, _userTimeZone));
				result.Add(optimizer);
			}

			return result;
		}
	}
}
