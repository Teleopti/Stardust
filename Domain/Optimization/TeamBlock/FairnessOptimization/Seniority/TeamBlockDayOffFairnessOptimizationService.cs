using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IDetermineTeamBlockWeekDayPriority _determineTeamBlockWeekDayPriority;
        private readonly ITeamBlockSwapValidator _teamBlockSwapValidator;
        private readonly ITeamBlockMatrixValidator _teamBlockMatrixValidator;

        public TeamBlockDayOffFairnessOptimizationService(IConstructTeamBlock constructTeamBlock, IDetermineTeamBlockWeekDayPriority determineTeamBlockWeekDayPriority, ITeamBlockSwapValidator teamBlockSwapValidator, ITeamBlockMatrixValidator teamBlockMatrixValidator)
        {
            _constructTeamBlock = constructTeamBlock;
            _determineTeamBlockWeekDayPriority = determineTeamBlockWeekDayPriority;
            _teamBlockSwapValidator = teamBlockSwapValidator;
            _teamBlockMatrixValidator = teamBlockMatrixValidator;
        }

        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
                            IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
        {
            _cancelMe = false;
            var instance = PrincipalAuthorization.Instance();
            if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction)) return;
            var tempSchedulingOptions = schedulingOptions;
            tempSchedulingOptions.UseTeamBlockPerOption = true;
            tempSchedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
            var listOfAllTeamBlock = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, tempSchedulingOptions);
            
            calcualteDayValueForSelectedPeriod(listOfAllTeamBlock);

            rearrangeDayOffAmongAagents();

            analyzeAndPerformPossibleSwaps();

        }

        private void calcualteDayValueForSelectedPeriod(IList<ITeamBlockInfo> listOfAllTeamBlock)
        {
            //populate the selected agents in a DS
            var teamBlockPriorityDefinitionInfoForWeekDay = _determineTeamBlockWeekDayPriority.CalculatePriority(listOfAllTeamBlock);

            //sort the DS according the the agent weekDayPriority
            foreach (var teamBlock in teamBlockPriorityDefinitionInfoForWeekDay.HighToLowSeniorityListBlockInfo)
            {
                if (!_teamBlockMatrixValidator.Validate(teamBlock)) continue;
                foreach (
                    var targetTeamBlock in teamBlockPriorityDefinitionInfoForWeekDay.ExtractAppropiateTeamBlock(teamBlock )
                    )
                {
                    if (_cancelMe) break;
                    if (!_teamBlockMatrixValidator.Validate(teamBlock)) continue;
                    if (_teamBlockSwapValidator.ValidateCanSwap(teamBlock, targetTeamBlock))
                    {
                        //perform swap
                        break;
                    }

                }
                if (_cancelMe) break;
                
               

				//var message = Resources.FairnessOptimizationOn + " " + Resources.Seniority + ": " + new Percent(analyzedTeamBlocks.Count / totalBlockCount);
				//OnReportProgress(message);
				//analyzedTeamBlocks.Add(teamBlockInfoHighSeniority);
	        
            }
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
