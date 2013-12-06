using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualNumberOfCategoryFairnessService
	{
		void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                             IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
		                             IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService);
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
		                                            IFilterPersonsForTotalDistribution filterPersonsForTotalDistribution)
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
		}

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                    IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var personListForTotalDistribution = _filterPersonsForTotalDistribution.Filter(allPersonMatrixList);
			
			var teamBlockListRaw = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons,
			                                                  schedulingOptions);

			var teamBlockListWithCorrectWorkFlowControlSet = _filterForEqualNumberOfCategoryFairness.Filter(teamBlockListRaw);
			
			var totalDistribution = _distributionForPersons.CreateSummary(personListForTotalDistribution, scheduleDictionary);
			var teamBlocksInSelection = _filterForTeamBlockInSelection.Filter(teamBlockListWithCorrectWorkFlowControlSet,
			                                                              selectedPersons, selectedPeriod);

			while (teamBlocksInSelection.Count > 0)
			{
				ITeamBlockInfo teamBlockInfoToWorkWith =
					_equalCategoryDistributionWorstTeamBlockDecider.FindBlockToWorkWith(totalDistribution, teamBlocksInSelection,
					                                                                    scheduleDictionary);
				teamBlocksInSelection.Remove(teamBlockInfoToWorkWith);

				var possibleTeamBlocksToSwapWith = _filterOnSwapableTeamBlocks.Filter(teamBlocksInSelection, teamBlockInfoToWorkWith);
				if(possibleTeamBlocksToSwapWith.Count == 0)
					continue;

				ITeamBlockInfo selectedTeamBlock =
					_equalCategoryDistributionBestTeamBlockDecider.FindBestSwap(teamBlockInfoToWorkWith, possibleTeamBlocksToSwapWith,
					                                                            totalDistribution, scheduleDictionary);
				if(selectedTeamBlock == null)
					continue;

				_teamBlockSwapper.Swap(teamBlockInfoToWorkWith, selectedTeamBlock, rollbackService, scheduleDictionary);
			}
		}
	}
}