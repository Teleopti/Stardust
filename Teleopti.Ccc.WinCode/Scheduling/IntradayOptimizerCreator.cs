using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
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
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IScheduleMatrixLockableBitArrayConverterEx _scheduleMatrixLockableBitArrayConverterEx;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataDivider _skillIntervalDataDivider;
		private readonly ISkillIntervalDataAggregator _skillIntervalDataAggregator;

		public IntradayOptimizer2Creator(
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
			IIntradayDecisionMaker decisionMaker,
			IScheduleService scheduleService,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IPersonSkillProvider personSkillProvider,
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
			IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataDivider skillIntervalDataDivider,
			ISkillIntervalDataAggregator skillIntervalDataAggregator)
		{
			_scheduleMatrixContainerList = scheduleMatrixContainerList;
			_workShiftStateContainerList = workShiftContainerList;
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_optimizerPreferences = optimizerPreferences;
			_rollbackService = rollbackService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_personSkillProvider = personSkillProvider;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_scheduleMatrixLockableBitArrayConverterEx = scheduleMatrixLockableBitArrayConverterEx;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataDivider = skillIntervalDataDivider;
			_skillIntervalDataAggregator = skillIntervalDataAggregator;
		}

		/// <summary>
		/// Creates the list of optimizers.
		/// </summary>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IList<IIntradayOptimizer2> Create()
		{
			IList<IIntradayOptimizer2> result = new List<IIntradayOptimizer2>();

			for (int index = 0; index < _scheduleMatrixContainerList.Count; index++)
			{

				IScheduleMatrixOriginalStateContainer originalStateContainer = _scheduleMatrixContainerList[index];

				IScheduleMatrixPro scheduleMatrix = originalStateContainer.ScheduleMatrix;

				IScheduleMatrixLockableBitArrayConverter matrixConverter =
					new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix, _scheduleMatrixLockableBitArrayConverterEx);

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

				INonBlendSkillCalculator nonBlendSkillCalculator = new NonBlendSkillCalculator();

				IDeleteSchedulePartService deleteSchedulePartService =
					new DeleteSchedulePartService(_schedulingResultStateHolder);
				IResourceOptimizationHelper resourceOptimizationHelper =
					new ResourceOptimizationHelper(_schedulingResultStateHolder,
                                                   new OccupiedSeatCalculator(), nonBlendSkillCalculator, _personSkillProvider, new PeriodDistributionService(), _currentTeleoptiPrincipal);
				IRestrictionExtractor restrictionExtractor =
					new RestrictionExtractor(_schedulingResultStateHolder);
				IEffectiveRestrictionCreator effectiveRestrictionCreator =
					new EffectiveRestrictionCreator(restrictionExtractor);

				IScheduleMatrixOriginalStateContainer workShiftStateContainer = _workShiftStateContainerList[index];

				var restrictionChecker = new RestrictionChecker();
				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrix, restrictionChecker, _optimizerPreferences, originalStateContainer);

				ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

				var schedulingOptions = schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
				var deleteAndResourceCalculateService = new DeleteAndResourceCalculateService(deleteSchedulePartService, resourceOptimizationHelper);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(resourceOptimizationHelper, 1, true, schedulingOptions.ConsiderShortBreaks);

				IIntradayOptimizer2 optimizer =
					new IntradayOptimizer2(
						dailyValueCalculator,
						personalSkillsDataExtractor,
						_decisionMaker,
						matrixConverter,
						_scheduleService,
						_optimizerPreferences,
						_rollbackService,
						resourceOptimizationHelper,
						effectiveRestrictionCreator,
						optimizerOverLimitDecider,
						workShiftStateContainer,
						schedulingOptionsCreator,
						mainShiftOptimizeActivitySpecificationSetter,
						deleteAndResourceCalculateService,
						resourceCalculateDelayer);

				result.Add(optimizer);
			}
			return result;
		}
	}
}
