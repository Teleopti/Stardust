using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizer2Creator : IIntradayOptimizer2Creator
	{
		private readonly IIntradayDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
		private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IMinWeekWorkTimeRule _minWeekWorkTimeRule;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public IntradayOptimizer2Creator(
			IIntradayDecisionMaker decisionMaker,
			IScheduleService scheduleService,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataDivider skillIntervalDataDivider,
			ISkillIntervalDataAggregator skillIntervalDataAggregator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IMinWeekWorkTimeRule minWeekWorkTimeRule,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataDivider = skillIntervalDataDivider;
			_skillIntervalDataAggregator = skillIntervalDataAggregator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_minWeekWorkTimeRule = minWeekWorkTimeRule;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		/// <summary>
		/// Creates the list of optimizers.
		/// </summary>
		/// <returns></returns>
		public IList<IIntradayOptimizer2> Create(IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList, IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			IList<IIntradayOptimizer2> result = new List<IIntradayOptimizer2>();

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

				IDeleteSchedulePartService deleteSchedulePartService =
					new DeleteSchedulePartService(()=>_schedulingResultStateHolder);
				
				IScheduleMatrixOriginalStateContainer workShiftStateContainer = workShiftContainerList[index];

				var restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrix.Person, scheduleMatrix.EffectivePeriodDays.First().Day);

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, originalStateContainer, dayOffOptimizationPreference);

				var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider, _minWeekWorkTimeRule);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

				var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
				var deleteAndResourceCalculateService = new DeleteAndResourceCalculateService(deleteSchedulePartService, _resourceOptimizationHelper);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);

				IIntradayOptimizer2 optimizer =
					new IntradayOptimizer2(
						dailyValueCalculator,
						personalSkillsDataExtractor,
						_decisionMaker,
						_scheduleService,
						optimizerPreferences,
						rollbackService,
						_resourceOptimizationHelper,
						_effectiveRestrictionCreator,
						optimizationLimits,
						workShiftStateContainer,
						schedulingOptionsCreator,
						mainShiftOptimizeActivitySpecificationSetter,
						deleteAndResourceCalculateService,
						resourceCalculateDelayer,
						scheduleMatrix);

				result.Add(optimizer);
			}
			return result;
		}
	}
}
