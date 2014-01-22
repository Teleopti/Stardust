using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    internal interface ITeamBlockDayOffFairnessOptimizationService
    {
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                                     ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
                                     IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService);
    }

    class TeamBlockDayOffFairnessOptimizationService : ITeamBlockDayOffFairnessOptimizationService
    {
        private bool _cancelMe;
        private IConstructTeamBlock _constructTeamBlock;
        private ISeniorityInfoExtractor _seniorityInfoExtractor;

        public TeamBlockDayOffFairnessOptimizationService(IConstructTeamBlock constructTeamBlock)
        {
            _constructTeamBlock = constructTeamBlock;
        }

        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
                            IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
        {
            _cancelMe = false;
            var instance = PrincipalAuthorization.Instance();
            if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction)) return;

            var listOfAllTeamBlock = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);
            
            calcualteDayValueForSelectedPeriod(listOfAllTeamBlock);

            rearrangeDayOffAmongAagents();

            analyzeAndPerformPossibleSwaps();

        }

        

        private void calcualteDayValueForSelectedPeriod(IList<ITeamBlockInfo> listOfAllTeamBlock)
        {
            //populate the selected agents in a DS
            var seniorityInfos = _seniorityInfoExtractor.ExtractSeniority(listOfAllTeamBlock);
            
            //sort the DS according the the agent priority

            //caculate the period value according to day value

            //perform swaps if validated
        }

        private void analyzeAndPerformPossibleSwaps()
        {
            throw new NotImplementedException();
        }

        private void rearrangeDayOffAmongAagents()
        {
            throw new NotImplementedException();
        }

        

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public virtual void OnReportProgress(string message)
        {
            var handler = ReportProgress;
            if (handler == null) return;
            var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
            handler(this, args);
            if (args.Cancel) _cancelMe = true;
        }
        
    }
}
