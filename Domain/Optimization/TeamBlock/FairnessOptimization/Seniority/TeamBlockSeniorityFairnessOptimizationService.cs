using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockSeniorityFairnessOptimizationService
    {
        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                                     ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
									 IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, ITeamBlockSwap teamBlockSwap);
    }

    public class TeamBlockSeniorityFairnessOptimizationService : ITeamBlockSeniorityFairnessOptimizationService
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly ITeamBlockSizeClassifier _teamBlockSizeClassifier;
        private readonly ITeamBlockListSwapAnalyzer _teamBlockListSwapAnalyzer;

        public TeamBlockSeniorityFairnessOptimizationService(IConstructTeamBlock constructTeamBlock,
                                          ITeamBlockSizeClassifier teamBlockSizeClassifier, ITeamBlockListSwapAnalyzer teamBlockListSwapAnalyzer)
        {
            _constructTeamBlock = constructTeamBlock;
            _teamBlockSizeClassifier = teamBlockSizeClassifier;
            _teamBlockListSwapAnalyzer = teamBlockListSwapAnalyzer;
        }

        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, ITeamBlockSwap teamBlockSwap)
        {
			var instance = PrincipalAuthorization.Instance();
	        if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction))
		        return;

            IList<ITeamBlockInfo> listOfAllTeamBlock = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod,
                                                                                     selectedPersons,
                                                                                     schedulingOptions);
            HashSet<IList<ITeamBlockInfo>> listOfMultipleLengthBlocks =
                _teamBlockSizeClassifier.SplitTeamBlockInfo(listOfAllTeamBlock);
            //Parallel.ForEach(listOfMultipleLengthBlocks.GetRandom(listOfAllTeamBlock.Count, true), teamBlockList => _teamBlockListSwapAnalyzer.AnalyzeTeamBlock(teamBlockList, shiftCategories));
            foreach (var teamBlockList in listOfMultipleLengthBlocks.GetRandom(listOfAllTeamBlock.Count, true))
            {
                _teamBlockListSwapAnalyzer.AnalyzeTeamBlock(teamBlockList, shiftCategories, scheduleDictionary, rollbackService, teamBlockSwap);
            }
        }

        
        //void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        //{
        //    OnDayScheduled(e);
        //}
        //public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        //protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        //{
        //    EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
        //    if (temp != null)
        //    {
        //        temp(this, scheduleServiceBaseEventArgs);
        //    }
        //    _cancelMe = scheduleServiceBaseEventArgs.Cancel;
        //}

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        //public void RaiseEventForTest(object sender, SchedulingServiceBaseEventArgs e)
        //{
        //    dayScheduled(sender, e);
        //}
    }
}