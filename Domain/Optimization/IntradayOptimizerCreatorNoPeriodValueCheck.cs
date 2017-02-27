using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Put the method override on IntradayOptimizer2Creator when done and remove this", Toggles.ResourcePlanner_IntradayNoDailyValueCheck_42767)]
	public class IntradayOptimizerCreatorNoPeriodValueCheck : IntradayOptimizer2Creator
	{
		private readonly IIntradayDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IIntradayOptimizeOneDayCallback _intradayOptimizeOneDayCallback;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

		public IntradayOptimizerCreatorNoPeriodValueCheck(
			IIntradayDecisionMaker decisionMaker,
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
			IMatrixListFactory matrixListFactory,
			PersonalSkillsProvider personalSkillsProvider,
			IUserTimeZone userTimeZone,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter) : base(decisionMaker, scheduleService, skillStaffPeriodToSkillIntervalDataMapper, skillIntervalDataDivider, skillIntervalDataAggregator, effectiveRestrictionCreator, resourceOptimizationHelper, deleteAndResourceCalculateService, intradayOptimizeOneDayCallback, schedulerStateHolder, scheduleDayChangeCallback, scheduleDayEquator, optimizerHelperHelper, matrixListFactory, personalSkillsProvider, userTimeZone, mainShiftOptimizeActivitySpecificationSetter)
		{
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_intradayOptimizeOneDayCallback = intradayOptimizeOneDayCallback;
			_schedulerStateHolder = schedulerStateHolder;
			_userTimeZone = userTimeZone;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
		}

		protected override IntradayOptimizer2 CreateIntradayOptimizer(IOptimizationPreferences optimizerPreferences,
			IScheduleResultDataExtractor personalSkillsDataExtractor, IScheduleMatrixPro scheduleMatrix,
			IScheduleResultDailyValueCalculator dailyValueCalculator, SchedulePartModifyAndRollbackService rollbackService,
			OptimizationLimits optimizationLimits, IScheduleMatrixOriginalStateContainer workShiftStateContainer,
			ISchedulingOptionsCreator schedulingOptionsCreator)
		{
			return new IntradayOptimizer2(personalSkillsDataExtractor, _decisionMaker, scheduleMatrix,
					new IntradayOptimizeOnedayNoPeriodValueCheck(dailyValueCalculator, _scheduleService, optimizerPreferences, rollbackService,
						_resourceOptimizationHelper,
						_effectiveRestrictionCreator, optimizationLimits, workShiftStateContainer, schedulingOptionsCreator,
						_mainShiftOptimizeActivitySpecificationSetter, _deleteAndResourceCalculateService,
						scheduleMatrix, _intradayOptimizeOneDayCallback, _schedulerStateHolder().SchedulingResultState, _userTimeZone));
		}
	}
}