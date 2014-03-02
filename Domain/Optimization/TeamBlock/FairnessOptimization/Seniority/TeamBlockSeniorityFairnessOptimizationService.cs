using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
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
									 IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
    }

    public class TeamBlockSeniorityFairnessOptimizationService : ITeamBlockSeniorityFairnessOptimizationService
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IDetermineTeamBlockPriority _determineTeamBlockPriority;
	    private readonly ITeamBlockPeriodValidator _teamBlockPeriodValidator;
	    private readonly ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
	    private readonly ITeamBlockSwap _teamBlockSwap;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		private bool _cancelMe;

        public TeamBlockSeniorityFairnessOptimizationService(IConstructTeamBlock constructTeamBlock, 
															IDetermineTeamBlockPriority determineTeamBlockPriority,
															ITeamBlockPeriodValidator teamBlockPeriodValidator,
															ITeamBlockSeniorityValidator teamBlockSeniorityValidator,
															ITeamBlockSwap teamBlockSwap,
															IFilterForTeamBlockInSelection filterForTeamBlockInSelection)
        {
            _constructTeamBlock = constructTeamBlock;
	        _determineTeamBlockPriority = determineTeamBlockPriority;
	        _teamBlockPeriodValidator = teamBlockPeriodValidator;
	        _teamBlockSeniorityValidator = teamBlockSeniorityValidator;
	        _teamBlockSwap = teamBlockSwap;
	        _filterForTeamBlockInSelection = filterForTeamBlockInSelection;
        }

	
        public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, 
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IOptimizationPreferences optimizationPreferences)
        {

	        var notSwapped = 0;
	        var loop = 1;

			_cancelMe = false;
	        while (!_cancelMe)
	        {
				var unSuccessfulSwaps = new List<ITeamBlockInfo>();
		        var listOfAllTeamBlock = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons,
		                                                               schedulingOptions.UseTeamBlockPerOption,
		                                                               schedulingOptions.BlockFinderTypeForAdvanceScheduling,
		                                                               schedulingOptions.GroupOnGroupPageForTeamBlockPer);

		        IList<ITeamBlockInfo> filteredTeamBlocks = listOfAllTeamBlock.Where(_teamBlockSeniorityValidator.ValidateSeniority).ToList();
				filteredTeamBlocks = _filterForTeamBlockInSelection.Filter(filteredTeamBlocks, selectedPersons, selectedPeriod);

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
				        if (teamBlockInfoHighSeniority.Equals(teamBlockInfoLowSeniority))
				        {
					        priorityList.Remove(teamBlockInfoHighSeniority);
					        break;
				        }

				        if (!_teamBlockPeriodValidator.ValidatePeriod(teamBlockInfoHighSeniority, teamBlockInfoLowSeniority))
					        continue;
				        var lowSeniorityPoints = teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(teamBlockInfoLowSeniority);
				        if (analyzedTeamBlocks.Contains(teamBlockInfoLowSeniority)) continue;
				        if (highSeniorityPoints >= lowSeniorityPoints)
				        {
					        priorityList.Remove(teamBlockInfoHighSeniority);
					        break;
				        }

						if (!_teamBlockSwap.Swap(teamBlockInfoHighSeniority, teamBlockInfoLowSeniority, rollbackService, scheduleDictionary, selectedPeriod, optimizationPreferences))
				        {
					        unSuccessfulSwaps.Add(teamBlockInfoHighSeniority);
					        continue;
				        }

				        unSuccessfulSwaps.Remove(teamBlockInfoHighSeniority);

				        teamBlockPriorityDefinitionInfo.SetShiftCategoryPoint(teamBlockInfoLowSeniority, highSeniorityPoints);

				        var highSeniorityIndex = priorityList.IndexOf(teamBlockInfoHighSeniority);
				        priorityList.Remove(teamBlockInfoLowSeniority);
				        priorityList[highSeniorityIndex - 1] = teamBlockInfoLowSeniority;
				        break;
			        }

			        double analyzed = analyzedTeamBlocks.Count/totalBlockCount;
					if(Math.Abs(analyzed - (int)analyzed) > 0.00001)
						continue;

			        var message = Resources.FairnessOptimizationOn + " " + Resources.Seniority + ": " + new Percent(analyzed);

			        if (loop > 1) message += " (" + loop + ")";

			        OnReportProgress(message);
			        analyzedTeamBlocks.Add(teamBlockInfoHighSeniority);
		        }

		        if (unSuccessfulSwaps.Count >= notSwapped && notSwapped > 0) _cancelMe = true;
		        if (unSuccessfulSwaps.Count == 0) _cancelMe = true;

		        notSwapped = unSuccessfulSwaps.Count;
				loop++;
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