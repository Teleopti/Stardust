using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
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

    	public IntradayOptimizer2Creator(
            IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
            IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
            IIntradayDecisionMaker decisionMaker,
            IScheduleService scheduleService,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _scheduleMatrixContainerList = scheduleMatrixContainerList;
            _workShiftStateContainerList = workShiftContainerList;
    	    _decisionMaker = decisionMaker;
        	_scheduleService = scheduleService;
        	_optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
        	_schedulingResultStateHolder = schedulingResultStateHolder;
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
                    new ScheduleMatrixLockableBitArrayConverter(scheduleMatrix);

                IScheduleResultDailyValueCalculator dailyValueCalculator =
                    new RelativeDailyStandardDeviationsByPersonalSkillsExtractor(scheduleMatrix, _optimizerPreferences.Advanced);
                IScheduleResultDataExtractor personalSkillsDataExtractor =
                    new RelativeDailyStandardDeviationsByPersonalSkillsExtractor(scheduleMatrix, _optimizerPreferences.Advanced);
				INonBlendSkillCalculator nonBlendSkillCalculator =
					new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection());

                IDeleteSchedulePartService deleteSchedulePartService =
					new DeleteSchedulePartService(_schedulingResultStateHolder);
                IResourceOptimizationHelper resourceOptimizationHelper =
					new ResourceOptimizationHelper(_schedulingResultStateHolder,
                                                   new OccupiedSeatCalculator(new SkillVisualLayerCollectionDictionaryCreator(), new SeatImpactOnPeriodForProjection()), nonBlendSkillCalculator);
                IRestrictionExtractor restrictionExtractor =
					new RestrictionExtractor(_schedulingResultStateHolder);
                IEffectiveRestrictionCreator effectiveRestrictionCreator =
                    new EffectiveRestrictionCreator(restrictionExtractor);

                IScheduleMatrixOriginalStateContainer workShiftStateContainer = _workShiftStateContainerList[index];

                var restrictionChecker = new RestrictionChecker();
                var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrix, restrictionChecker, _optimizerPreferences, originalStateContainer);

                ISchedulingOptionsCreator schedulingOptionsCreator = new SchedulingOptionsCreator();

                IIntradayOptimizer2 optimizer =
                    new IntradayOptimizer2(
                        dailyValueCalculator,
                        personalSkillsDataExtractor,
                        _decisionMaker,
                        matrixConverter,
						_scheduleService,
                        _optimizerPreferences,
                        _rollbackService,
                        deleteSchedulePartService,
						resourceOptimizationHelper,
                        effectiveRestrictionCreator,
                        new ResourceCalculateDaysDecider(),  
                        optimizerOverLimitDecider,
                        workShiftStateContainer,
                        schedulingOptionsCreator);

                result.Add(optimizer);
            }
            return result;
        }
    }
}
