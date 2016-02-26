using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizer2Creator
	{
		private readonly IIntradayDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
		private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IMinWeekWorkTimeRule _minWeekWorkTimeRule;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IIntradayOptimizeOneDayCallback _intradayOptimizeOneDayCallback;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly IMatrixListFactory _matrixListFactory;

		public IntradayOptimizer2Creator(
			IIntradayDecisionMaker decisionMaker,
			IScheduleService scheduleService,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataDivider skillIntervalDataDivider,
			ISkillIntervalDataAggregator skillIntervalDataAggregator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IMinWeekWorkTimeRule minWeekWorkTimeRule,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IIntradayOptimizeOneDayCallback intradayOptimizeOneDayCallback,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			IScheduleDayEquator scheduleDayEquator,
			IOptimizerHelperHelper optimizerHelperHelper, 
			IMatrixListFactory matrixListFactory)
		{
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataDivider = skillIntervalDataDivider;
			_skillIntervalDataAggregator = skillIntervalDataAggregator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_minWeekWorkTimeRule = minWeekWorkTimeRule;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_intradayOptimizeOneDayCallback = intradayOptimizeOneDayCallback;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleDayEquator = scheduleDayEquator;
			_optimizerHelperHelper = optimizerHelperHelper;
			_matrixListFactory = matrixListFactory;
		}

		public IEnumerable<IIntradayOptimizer2> Create(DateOnlyPeriod period, IEnumerable<IScheduleDay> scheduleDays, IOptimizationPreferences optimizerPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var scheduleMatrixes = _matrixListFactory.CreateMatrixListForSelection(scheduleDays.ToList()); //ta bort enumerable
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
				IScheduleMatrixOriginalStateContainer originalStateContainer = scheduleMatrixContainerList[index];

				IScheduleMatrixPro scheduleMatrix = originalStateContainer.ScheduleMatrix;

				IScheduleResultDailyValueCalculator dailyValueCalculator = new RelativeDailyValueByPersonalSkillsExtractor(scheduleMatrix,
				                                                                                                           optimizerPreferences.Advanced,
				                                                                                                           _skillStaffPeriodToSkillIntervalDataMapper,
				                                                                                                           _skillIntervalDataDivider,
				                                                                                                           _skillIntervalDataAggregator);
				IScheduleResultDataExtractor personalSkillsDataExtractor = new RelativeDailyValueByPersonalSkillsExtractor(scheduleMatrix,
				                                                                                                           optimizerPreferences.Advanced,
				                                                                                                           _skillStaffPeriodToSkillIntervalDataMapper,
				                                                                                                           _skillIntervalDataDivider,
				                                                                                                           _skillIntervalDataAggregator);
				
				IScheduleMatrixOriginalStateContainer workShiftStateContainer = workShiftContainerList[index];

				var restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrix.Person, scheduleMatrix.EffectivePeriodDays.First().Day);

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateContainer, dayOffOptimizationPreference);

				var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider, _minWeekWorkTimeRule);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

				var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);

				var optimizer = new IntradayOptimizer2(personalSkillsDataExtractor, _decisionMaker,  scheduleMatrix,
					new IntradayOptimizeOneday(dailyValueCalculator, _scheduleService, optimizerPreferences, rollbackService,
						_resourceOptimizationHelper,
						_effectiveRestrictionCreator, optimizationLimits, workShiftStateContainer, schedulingOptionsCreator,
						mainShiftOptimizeActivitySpecificationSetter, _deleteAndResourceCalculateService, resourceCalculateDelayer,
						scheduleMatrix, _intradayOptimizeOneDayCallback));

				result.Add(optimizer);
			}

			return result;
		}
	}
}
