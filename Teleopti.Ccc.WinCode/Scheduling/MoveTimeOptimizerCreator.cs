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
    public class MoveTimeOptimizerCreator : IMoveTimeOptimizerCreator
    {
        private readonly IList<IScheduleMatrixOriginalStateContainer> _scheduleMatrixContainerList;
        private readonly IList<IScheduleMatrixOriginalStateContainer> _workShiftContainerList;
        private readonly IMoveTimeDecisionMaker _decisionMaker;
    	private readonly IScheduleService _scheduleService;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
    	private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

        public MoveTimeOptimizerCreator(
            IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
            IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
            IMoveTimeDecisionMaker decisionMaker,
            IScheduleService scheduleService,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _scheduleMatrixContainerList = scheduleMatrixContainerList;
            _workShiftContainerList = workShiftContainerList;
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
		public IList<IMoveTimeOptimizer> Create()
        {
        	IList<IMoveTimeOptimizer> result = new List<IMoveTimeOptimizer>();

            for (int index = 0; index < _scheduleMatrixContainerList.Count; index++)
            {

                IScheduleMatrixOriginalStateContainer scheduleMatrixContainer = _scheduleMatrixContainerList[index];
                
        	    IScheduleMatrixPro scheduleMatrixPro = scheduleMatrixContainer.ScheduleMatrix;

        		IScheduleMatrixLockableBitArrayConverter matrixConverter =
                    new ScheduleMatrixLockableBitArrayConverter(scheduleMatrixPro);

                IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider(_optimizerPreferences.Advanced);
                IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro);

                IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
        	    IPeriodValueCalculator periodValueCalculator =
        	        periodValueCalculatorProvider.CreatePeriodValueCalculator(_optimizerPreferences.Advanced, personalSkillsDataExtractor);

                IDeleteSchedulePartService deleteSchedulePartService =
					new DeleteSchedulePartService(_schedulingResultStateHolder);

        		IOccupiedSeatCalculator occupiedSeatCalculator =
        			new OccupiedSeatCalculator(new SkillVisualLayerCollectionDictionaryCreator(),
        			                           new SeatImpactOnPeriodForProjection());

        		INonBlendSkillCalculator nonBlendSkillCalculator =
        			new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection());

        		IResourceOptimizationHelper resourceOptimizationHelper =
					new ResourceOptimizationHelper(_schedulingResultStateHolder, occupiedSeatCalculator, nonBlendSkillCalculator);

        		IRestrictionExtractor restrictionExtractor =
					new RestrictionExtractor(_schedulingResultStateHolder);

				IEffectiveRestrictionCreator effectiveRestrictionCreator = new EffectiveRestrictionCreator(restrictionExtractor,
        					new KeepRestrictionCreator());

                IScheduleMatrixOriginalStateContainer workShiftContainer = _workShiftContainerList[index];

                var restrictionChecker = new RestrictionChecker();
                var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(scheduleMatrixContainer, restrictionChecker, _optimizerPreferences);

                var schedulingOptionsCreator = new SchedulingOptionsSynchronizer();

        		IMoveTimeOptimizer optimizer =
        			new MoveTimeOptimizer(
                        periodValueCalculator,
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
                        workShiftContainer, 
                        optimizerOverLimitDecider, 
                        schedulingOptionsCreator);

        		result.Add(optimizer);
        	}
        	return result;
        }
    }
}
