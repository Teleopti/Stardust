using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public  interface ITeamBlockDayOffFairnessOptimizationService
    {
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                                     ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
                                     IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService);
    }

    public class TeamBlockDayOffFairnessOptimizationService : ITeamBlockDayOffFairnessOptimizationService
    {
        private bool _cancelMe;
        private readonly IConstructTeamBlock _constructTeamBlock;
        private IDayOffStep1 _dayOffStep1;
        private IDayOffStep2 _dayOffStep2;

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

            _dayOffStep1.PerformStep1(schedulingOptions,allPersonMatrixList,selectedPeriod,selectedPersons );

            _dayOffStep2.PerformStep2();
            rearrangeDayOffAmongAagents();

            if(!schedulingOptions.UseSameDayOffs )
                analyzeAndPerformPossibleSwaps(selectedPersons, allPersonMatrixList, selectedPeriod, schedulingOptions);

        }


        private void analyzeAndPerformPossibleSwaps(IList<IPerson> selectedPersons, IList<IScheduleMatrixPro> modifiedMatrixList, DateOnlyPeriod selectedPeriod, ISchedulingOptions schedulingOptions )
        {
            //step 2 test code
            var tempSchedulingOptions = schedulingOptions;
            tempSchedulingOptions.UseBlockOptimizing = BlockFinderType.SingleDay ;
            var singleAgentGroupPage = new GroupPageLight {Key = "SingleAgentTeam", Name = "SingleAgentTeam"};
            tempSchedulingOptions.GroupOnGroupPageForTeamBlockPer = singleAgentGroupPage;
			var listOfAllTeamBlock = _constructTeamBlock.Construct(modifiedMatrixList, selectedPeriod, selectedPersons, schedulingOptions.UseTeamBlockPerOption,
																 schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 schedulingOptions.GroupOnGroupPageForTeamBlockPer);
            //var seniorityInfos = _seniorityExtractor.ExtractSeniority(listOfAllTeamBlock);

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
