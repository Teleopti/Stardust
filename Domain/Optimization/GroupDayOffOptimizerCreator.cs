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
            IDaysOffPreferences daysOffPreferences);
    }

    public class GroupDayOffOptimizerCreator : IGroupDayOffOptimizerCreator
    {
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IGroupSchedulingService _groupSchedulingService;
        private readonly IGroupMatrixHelper _groupMatrixHelper;
    	private readonly IGroupOptimizationValidatorRunner _groupOptimizationValidatorRunner;
    	private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

    	public GroupDayOffOptimizerCreator(
            IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
            ILockableBitArrayChangesTracker changesTracker, 
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
            IGroupSchedulingService groupSchedulingService, 
            IGroupMatrixHelper groupMatrixHelper,
			IGroupOptimizationValidatorRunner groupOptimizationValidatorRunner,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _changesTracker = changesTracker;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _groupSchedulingService = groupSchedulingService;
            _groupMatrixHelper = groupMatrixHelper;
        	_groupOptimizationValidatorRunner = groupOptimizationValidatorRunner;
    		_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
        }

		public IGroupDayOffOptimizer CreateDayOffOptimizer(
			IScheduleMatrixLockableBitArrayConverter converter,
			IDayOffDecisionMaker decisionMaker,
			IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
			IDaysOffPreferences daysOffPreferences)
		{
			return new GroupDayOffOptimizer(converter,
			                                decisionMaker,
			                                _scheduleResultDataExtractorProvider,
			                                daysOffPreferences,
			                                dayOffDecisionMakerExecuter,
			                                _changesTracker,
			                                _schedulePartModifyAndRollbackService,
			                                _groupSchedulingService,
			                                _groupMatrixHelper,
			                                _groupOptimizationValidatorRunner,
			                                _groupPersonBuilderForOptimization);

		}
    }
}