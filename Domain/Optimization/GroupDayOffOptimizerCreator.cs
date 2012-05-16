using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupDayOffOptimizerCreator
    {
        IGroupDayOffOptimizer CreateDayOffOptimizer(
            IScheduleMatrixLockableBitArrayConverter converter, 
            IDayOffDecisionMaker decisionMaker, 
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            IDaysOffPreferences daysOffPreferences, 
            IList<IDayOffLegalStateValidator> validatorList, 
            IList<IPerson> allSelectedPersons,
			bool useSameDaysOff);
    }

    public class GroupDayOffOptimizerCreator : IGroupDayOffOptimizerCreator
    {
		//private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IGroupSchedulingService _groupSchedulingService;
        private readonly IGroupPersonPreOptimizationChecker _groupPersonPreOptimizationChecker;
        private readonly IGroupMatrixHelper _groupMatrixHelper;

        public GroupDayOffOptimizerCreator(
            //IOptimizationPreferences optimizerPreferences, 
            IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
            ILockableBitArrayChangesTracker changesTracker, 
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
            IGroupSchedulingService groupSchedulingService, 
            IGroupPersonPreOptimizationChecker groupPersonPreOptimizationChecker, 
            IGroupMatrixHelper groupMatrixHelper)
        {
            //_optimizerPreferences = optimizerPreferences;
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _changesTracker = changesTracker;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _groupSchedulingService = groupSchedulingService;
            _groupPersonPreOptimizationChecker = groupPersonPreOptimizationChecker;
            _groupMatrixHelper = groupMatrixHelper;
        }

        public IGroupDayOffOptimizer CreateDayOffOptimizer(
            IScheduleMatrixLockableBitArrayConverter converter, 
            IDayOffDecisionMaker decisionMaker,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter, 
            IDaysOffPreferences daysOffPreferences, 
            IList<IDayOffLegalStateValidator> validatorList, 
            IList<IPerson> allSelectedPersons,
			bool useSameDaysOff)
        {
			if (useSameDaysOff)
				return new GroupDayOffOptimizer(converter,
									decisionMaker,
									_scheduleResultDataExtractorProvider,
									daysOffPreferences,
									dayOffDecisionMakerExecuter,
									_changesTracker,
									_schedulePartModifyAndRollbackService,
									_groupSchedulingService,
									validatorList,
									allSelectedPersons,
									_groupPersonPreOptimizationChecker,
									_groupMatrixHelper);

            return new GroupDayOffSingleOptimizer(converter,
                                    decisionMaker,
                                    _scheduleResultDataExtractorProvider,
                                    daysOffPreferences,
                                    dayOffDecisionMakerExecuter,
                                    _changesTracker,
                                    _schedulePartModifyAndRollbackService,
                                    _groupSchedulingService,
                                    validatorList,
                                    allSelectedPersons,
                                    _groupPersonPreOptimizationChecker.GroupPersonBuilder,
                                    _groupMatrixHelper);
        }
    }
}