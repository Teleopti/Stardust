using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class IntradayOptimizer2Creator : IIntradayOptimizer2Creator
	{
		private readonly IList<IScheduleMatrixOriginalStateContainer> _scheduleMatrixContainerList;
		private readonly IList<IScheduleMatrixOriginalStateContainer> _workShiftStateContainerList;
		private readonly IIntradayDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
		private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IMinWeekWorkTimeRule _minWeekWorkTimeRule;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;

		public IntradayOptimizer2Creator(
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
			IIntradayDecisionMaker decisionMaker,
			IScheduleService scheduleService,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataDivider skillIntervalDataDivider,
			ISkillIntervalDataAggregator skillIntervalDataAggregator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IMinWeekWorkTimeRule minWeekWorkTimeRule,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_scheduleMatrixContainerList = scheduleMatrixContainerList;
			_workShiftStateContainerList = workShiftContainerList;
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_optimizerPreferences = optimizerPreferences;
			_rollbackService = rollbackService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataDivider = skillIntervalDataDivider;
			_skillIntervalDataAggregator = skillIntervalDataAggregator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_minWeekWorkTimeRule = minWeekWorkTimeRule;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_dayOffOptimizationPreferenceProvider = dayOffOptimizationPreferenceProvider;
		}

		/// <summary>
		/// Creates the list of optimizers.
		/// </summary>
		/// <returns></returns>
		public IList<IIntradayOptimizer2> Create()
		{
			IList<IIntradayOptimizer2> result = new List<IIntradayOptimizer2>();

			for (int index = 0; index < _scheduleMatrixContainerList.Count; index++)
			{
				IScheduleMatrixOriginalStateContainer originalStateContainer = _scheduleMatrixContainerList[index];

				IScheduleMatrixPro scheduleMatrix = originalStateContainer.ScheduleMatrix;

				IScheduleResultDailyValueCalculator dailyValueCalculator = new RelativeDailyValueByPersonalSkillsExtractor(scheduleMatrix,
				                                                                                                           _optimizerPreferences.Advanced,
				                                                                                                           _skillStaffPeriodToSkillIntervalDataMapper,
				                                                                                                           _skillIntervalDataDivider,
				                                                                                                           _skillIntervalDataAggregator);
				IScheduleResultDataExtractor personalSkillsDataExtractor = new RelativeDailyValueByPersonalSkillsExtractor(scheduleMatrix,
				                                                                                                           _optimizerPreferences.Advanced,
				                                                                                                           _skillStaffPeriodToSkillIntervalDataMapper,
				                                                                                                           _skillIntervalDataDivider,
				                                                                                                           _skillIntervalDataAggregator);

				IDeleteSchedulePartService deleteSchedulePartService =
					new DeleteSchedulePartService(()=>_schedulingResultStateHolder);
				
				IScheduleMatrixOriginalStateContainer workShiftStateContainer = _workShiftStateContainerList[index];

				var restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = _dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrix.Person, scheduleMatrix.EffectivePeriodDays.First().Day);

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, _optimizerPreferences, originalStateContainer, dayOffOptimizationPreference);

				var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider, _minWeekWorkTimeRule);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

				var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
				var deleteAndResourceCalculateService = new DeleteAndResourceCalculateService(deleteSchedulePartService, _resourceOptimizationHelper);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);

				IIntradayOptimizer2 optimizer =
					new IntradayOptimizer2(
						dailyValueCalculator,
						personalSkillsDataExtractor,
						_decisionMaker,
						_scheduleService,
						_optimizerPreferences,
						_rollbackService,
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
