using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualNumberOfCategoryFairnessService
	{
		void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                             IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
		                             IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService);

		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class EqualNumberOfCategoryFairnessService : IEqualNumberOfCategoryFairnessService
	{
		private readonly IConstructTeamBlock _constructTeamBlock;
		private readonly IDistributionForPersons _distributionForPersons;
		private readonly IFilterForEqualNumberOfCategoryFairness _filterForEqualNumberOfCategoryFairness;
		private readonly IFilterForTeamBlockInSelection _filterForTeamBlockInSelection;
		private readonly IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlocks;
		private readonly ITeamBlockSwapper _teamBlockSwapper;
		private readonly IEqualCategoryDistributionBestTeamBlockDecider _equalCategoryDistributionBestTeamBlockDecider;
		private readonly IEqualCategoryDistributionWorstTeamBlockDecider _equalCategoryDistributionWorstTeamBlockDecider;
		private readonly IFilterPersonsForTotalDistribution _filterPersonsForTotalDistribution;
		private readonly IFilterForFullyScheduledBlocks _filterForFullyScheduledBlocks;
		private bool _cancelMe;

		public EqualNumberOfCategoryFairnessService(IConstructTeamBlock constructTeamBlock,
		                                            IDistributionForPersons distributionForPersons,
		                                            IFilterForEqualNumberOfCategoryFairness
			                                            filterForEqualNumberOfCategoryFairness,
		                                            IFilterForTeamBlockInSelection filterForTeamBlockInSelection,
		                                            IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks,
		                                            ITeamBlockSwapper teamBlockSwapper,
		                                            IEqualCategoryDistributionBestTeamBlockDecider
			                                            equalCategoryDistributionBestTeamBlockDecider,
		                                            IEqualCategoryDistributionWorstTeamBlockDecider
			                                            equalCategoryDistributionWorstTeamBlockDecider,
		                                            IFilterPersonsForTotalDistribution filterPersonsForTotalDistribution,
													IFilterForFullyScheduledBlocks filterForFullyScheduledBlocks)
		{
			_constructTeamBlock = constructTeamBlock;
			_distributionForPersons = distributionForPersons;
			_filterForEqualNumberOfCategoryFairness = filterForEqualNumberOfCategoryFairness;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
			_teamBlockSwapper = teamBlockSwapper;
			_equalCategoryDistributionBestTeamBlockDecider = equalCategoryDistributionBestTeamBlockDecider;
			_equalCategoryDistributionWorstTeamBlockDecider = equalCategoryDistributionWorstTeamBlockDecider;
			_filterPersonsForTotalDistribution = filterPersonsForTotalDistribution;
			_filterForFullyScheduledBlocks = filterForFullyScheduledBlocks;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                    IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
		{
			_cancelMe = false;
			var personListForTotalDistribution = _filterPersonsForTotalDistribution.Filter(allPersonMatrixList);
			
			var teamBlockListRaw = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons,
			                                                  schedulingOptions);

			var teamBlockListWithCorrectWorkFlowControlSet = _filterForEqualNumberOfCategoryFairness.Filter(teamBlockListRaw);
			
			var totalDistribution = _distributionForPersons.CreateSummary(personListForTotalDistribution, scheduleDictionary);
			var teamBlocksInSelection = _filterForTeamBlockInSelection.Filter(teamBlockListWithCorrectWorkFlowControlSet,
			                                                              selectedPersons, selectedPeriod);

			var blocksToWorkWith = _filterForFullyScheduledBlocks.IsFullyScheduled(teamBlocksInSelection, scheduleDictionary);
			
			double totalBlockCount = blocksToWorkWith.Count;
			while (blocksToWorkWith.Count > 0 && !_cancelMe)
			{
				ITeamBlockInfo teamBlockInfoToWorkWith =
					_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(totalDistribution, blocksToWorkWith,
					                                                                    scheduleDictionary);
				blocksToWorkWith.Remove(teamBlockInfoToWorkWith);

				var possibleTeamBlocksToSwapWith = _filterOnSwapableTeamBlocks.Filter(blocksToWorkWith, teamBlockInfoToWorkWith);
				if(possibleTeamBlocksToSwapWith.Count == 0)
					continue;

				ITeamBlockInfo selectedTeamBlock =
					_equalCategoryDistributionBestTeamBlockDecider.FindBestSwap(teamBlockInfoToWorkWith, possibleTeamBlocksToSwapWith,
					                                                            totalDistribution, scheduleDictionary);
				if(selectedTeamBlock == null)
					continue;

				_teamBlockSwapper.TrySwap(teamBlockInfoToWorkWith, selectedTeamBlock, rollbackService, scheduleDictionary);

				var message = Resources.FairnessOptimizationOn + " " + Resources.EqualOfEachShiftCategory + ": " +
							  new Percent((totalBlockCount - blocksToWorkWith.Count) / totalBlockCount);

				OnReportProgress(message);
			}
		}

		public virtual void OnReportProgress(string message)
		{
			EventHandler<ResourceOptimizerProgressEventArgs> handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}
	}
}