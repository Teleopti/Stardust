using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    public interface ITeamBlockSeniorityFairnessOptimizationService
    {
        void Execute(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, SchedulingOptions schedulingOptions, 
					IList<IShiftCategory> shiftCategories, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, 
					IOptimizationPreferences optimizationPreferences, bool scheduleSeniority11111, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
    }

    public class TeamBlockSeniorityFairnessOptimizationService : ITeamBlockSeniorityFairnessOptimizationService
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly DetermineTeamBlockPriority _determineTeamBlockPriority;
	    private readonly ITeamBlockPeriodValidator _teamBlockPeriodValidator;
	    private readonly ITeamBlockSeniorityValidator _teamBlockSeniorityValidator;
	    private readonly ITeamBlockSwap _teamBlockSwap;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		
        public TeamBlockSeniorityFairnessOptimizationService(IConstructTeamBlock constructTeamBlock, 
															DetermineTeamBlockPriority determineTeamBlockPriority,
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


	    public void Execute(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		    IEnumerable<IPerson> selectedPersons, SchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories,
		    IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService,
		    IOptimizationPreferences optimizationPreferences, bool scheduleSeniority11111, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
	    {

		    var notSwapped = 0;
		    var loop = 1;
		    var cancelMe = false;

		    while (!cancelMe)
		    {
			    var unSuccessfulSwaps = new List<ITeamBlockInfo>();
			    var listOfAllTeamBlock = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons,
				    schedulingOptions.BlockFinder(),
				    schedulingOptions.GroupOnGroupPageForTeamBlockPer);

				IList<ITeamBlockInfo> filteredTeamBlocks = new List<ITeamBlockInfo>();
			    foreach (var teamBlockInfo in listOfAllTeamBlock)
			    {
				    if (_teamBlockSeniorityValidator.ValidateSeniority(teamBlockInfo))
				    filteredTeamBlocks.Add(teamBlockInfo);
			    }
			    filteredTeamBlocks = _filterForTeamBlockInSelection.Filter(filteredTeamBlocks, selectedPersons, selectedPeriod);

			    var teamBlockPriorityDefinitionInfo = _determineTeamBlockPriority.CalculatePriority(filteredTeamBlocks,
				    shiftCategories);
			    var analyzedTeamBlocks = new List<ITeamBlockInfo>();
			    var priorityList = teamBlockPriorityDefinitionInfo.HighToLowShiftCategoryPriority();
			    double totalBlockCount = priorityList.Count;

			    foreach (var teamBlockInfoHighSeniority in teamBlockPriorityDefinitionInfo.HighToLowSeniorityListBlockInfo)
			    {
				    if (cancelMe) break;

				    var highSeniorityPoints =
					    teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(teamBlockInfoHighSeniority);

				    var highSenioritySeniority = teamBlockPriorityDefinitionInfo.GetSeniorityOfBlock(teamBlockInfoHighSeniority);

				    foreach (var teamBlockInfoLowSeniority in priorityList)
				    {
					    if (teamBlockInfoHighSeniority.Equals(teamBlockInfoLowSeniority))
					    {
						    priorityList.Remove(teamBlockInfoHighSeniority);
						    break;
					    }

					    var lowSenioritySeniority = teamBlockPriorityDefinitionInfo.GetSeniorityOfBlock(teamBlockInfoLowSeniority);

						if (Math.Abs(highSenioritySeniority - lowSenioritySeniority) < 1.0) continue;
		

					    if (!_teamBlockPeriodValidator.ValidatePeriod(teamBlockInfoHighSeniority, teamBlockInfoLowSeniority))
						    continue;
					    var lowSeniorityPoints =
						    teamBlockPriorityDefinitionInfo.GetShiftCategoryPriorityOfBlock(teamBlockInfoLowSeniority);
					    if (analyzedTeamBlocks.Contains(teamBlockInfoLowSeniority)) continue;
					    if (highSeniorityPoints >= lowSeniorityPoints)
					    {
						    priorityList.Remove(teamBlockInfoHighSeniority);
						    break;
					    }

					    if (
						    !_teamBlockSwap.Swap(teamBlockInfoHighSeniority, teamBlockInfoLowSeniority, rollbackService,
							    scheduleDictionary, selectedPeriod, optimizationPreferences, dayOffOptimizationPreferenceProvider))
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
				    if (Math.Abs(analyzed - (int) analyzed) > 0.00001)
					    continue;

				    var message = Resources.FairnessOptimizationOn + " " + Resources.Seniority + ": " + new Percent(analyzed);

				    if (loop > 1) message += " (" + loop + ")";

					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, message, optimizationPreferences.Advanced.RefreshScreenInterval, ()=>cancelMe=true));
					if (progressResult.ShouldCancel) cancelMe = true;

				    analyzedTeamBlocks.Add(teamBlockInfoHighSeniority);
			    }

			    if (unSuccessfulSwaps.Count >= notSwapped && notSwapped > 0) cancelMe = true;
			    if (unSuccessfulSwaps.Count == 0) cancelMe = true;

			    notSwapped = unSuccessfulSwaps.Count;
			    loop++;
		    }
	    }

	    private CancelSignal onReportProgress(ResourceOptimizerProgressEventArgs args)
		{
			var handler = ReportProgress;
		    if (handler != null)
		    {
			    handler(this, args);
			    if (args.Cancel) return new CancelSignal {ShouldCancel = true};
		    }
			return new CancelSignal();
		}
    }
}