using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockSeniorityFairnessOptimizationService
    {
        void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                                     ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
									 IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
    }

    public class TeamBlockSeniorityFairnessOptimizationService : ITeamBlockSeniorityFairnessOptimizationService
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IDetermineTeamBlockPriority _determineTeamBlockPriority;
	    private readonly ITeamBlockPeriodValidator _teamBlockPeriodValidator;
	    private readonly ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
	    private readonly ITeamBlockSwap _teamBlockSwap;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		private bool _cancelMe;

        public TeamBlockSeniorityFairnessOptimizationService(IConstructTeamBlock constructTeamBlock, 
															IDetermineTeamBlockPriority determineTeamBlockPriority,
															ITeamBlockPeriodValidator teamBlockPeriodValidator,
															ITeamBlockSeniorityValidator teamBlockSeniorityValidator,
															ITeamBlockSwap teamBlockSwap)
        {
            _constructTeamBlock = constructTeamBlock;
	        _determineTeamBlockPriority = determineTeamBlockPriority;
	        _teamBlockPeriodValidator = teamBlockPeriodValidator;
	        _teamBlockSeniorityValidator = teamBlockSeniorityValidator;
	        _teamBlockSwap = teamBlockSwap;
        }

	
        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
        {
			_cancelMe = false;
			var instance = PrincipalAuthorization.Instance();
	        if (!instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.UnderConstruction)) return;
            var listOfAllTeamBlock = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons, schedulingOptions);

			var filteredTeamBlocks = listOfAllTeamBlock.Where(_teamBlockSeniorityValidator.ValidateSeniority).ToList();
			var teamBlockPriorityDefinitionInfo = _determineTeamBlockPriority.CalculatePriority(filteredTeamBlocks, shiftCategories);
	        var analyzedTeamBlocks = new List<ITeamBlockInfo>();
			var priorityList = teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriority();
	        double totalBlockCount = priorityList.Count;

	        foreach (var teamBlockInfoHighSeniority in teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo)
	        {
		        if (_cancelMe) break;
		        var highSeniorityPoints = teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(teamBlockInfoHighSeniority);

		        foreach (var teamBlockInfoLowSeniority in priorityList)
		        {
					if (!_teamBlockPeriodValidator.ValidatePeriod(teamBlockInfoHighSeniority, teamBlockInfoLowSeniority)) continue;
			        var lowSeniorityPoints = teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(teamBlockInfoLowSeniority);
			        if (analyzedTeamBlocks.Contains(teamBlockInfoLowSeniority)) continue;
			        if (highSeniorityPoints >= lowSeniorityPoints)
			        {
				        priorityList.Remove(teamBlockInfoHighSeniority);
				        break;
			        }
			        if (teamBlockInfoHighSeniority.Equals(teamBlockInfoLowSeniority)) continue;
			        if (!_teamBlockSwap.Swap(teamBlockInfoHighSeniority, teamBlockInfoLowSeniority, rollbackService,scheduleDictionary)) continue;

			        teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(teamBlockInfoLowSeniority, highSeniorityPoints);

			        var highSeniorityIndex = priorityList.IndexOf(teamBlockInfoHighSeniority);
			        priorityList.Remove(teamBlockInfoLowSeniority);
			        priorityList[highSeniorityIndex - 1] = teamBlockInfoLowSeniority;
			        break;
		        }

				var message = Resources.FairnessOptimizationOn + " " + Resources.Seniority + ": " + new Percent(analyzedTeamBlocks.Count / totalBlockCount);
				OnReportProgress(message);
				analyzedTeamBlocks.Add(teamBlockInfoHighSeniority);
	        }
        }

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