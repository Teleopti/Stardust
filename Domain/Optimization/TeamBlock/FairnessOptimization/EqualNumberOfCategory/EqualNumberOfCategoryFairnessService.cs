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

		public EqualNumberOfCategoryFairnessService(IConstructTeamBlock constructTeamBlock, IDistributionForPersons distributionForPersons, IFilterForEqualNumberOfCategoryFairness filterForEqualNumberOfCategoryFairness, IFilterForTeamBlockInSelection filterForTeamBlockInSelection, IFilterOnSwapableTeamBlocks filterOnSwapableTeamBlocks, ITeamBlockSwapper teamBlockSwapper, IEqualCategoryDistributionBestTeamBlockDecider equalCategoryDistributionBestTeamBlockDecider)
		{
			_constructTeamBlock = constructTeamBlock;
			_distributionForPersons = distributionForPersons;
			_filterForEqualNumberOfCategoryFairness = filterForEqualNumberOfCategoryFairness;
			_filterForTeamBlockInSelection = filterForTeamBlockInSelection;
			_filterOnSwapableTeamBlocks = filterOnSwapableTeamBlocks;
			_teamBlockSwapper = teamBlockSwapper;
			_equalCategoryDistributionBestTeamBlockDecider = equalCategoryDistributionBestTeamBlockDecider;
		}

		public void Execute(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                    IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, 
							IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var teamBlockListRaw = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod, selectedPersons,
			                                                  schedulingOptions);

			var teamBlockListWithCorrectWorkFlowControlSet = _filterForEqualNumberOfCategoryFairness.Filter(teamBlockListRaw);
			
			var personListForTotalDistribution = new HashSet<IPerson>();
			foreach (var teamBlockInfo in teamBlockListWithCorrectWorkFlowControlSet)
			{
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
				{
					personListForTotalDistribution.Add(groupMember);
				}
			}
			
			var totalDistribution = _distributionForPersons.CreateSummary(personListForTotalDistribution, scheduleDictionary);
			var teamBlockInfoList = _filterForTeamBlockInSelection.Filter(teamBlockListWithCorrectWorkFlowControlSet,
			                                                              selectedPersons, selectedPeriod);

			while (teamBlockInfoList.Count > 0)
			{

				//to standalone class
				ITeamBlockInfo teamBlockInfoToWorkWith = null;
				var teamBlockInfoDistributionValue = 0d;
				foreach (var teamBlockInfo in teamBlockInfoList)
				{
					var distribution = _distributionForPersons.CreateSummary(teamBlockInfo.TeamInfo.GroupPerson.GroupMembers,
					                                                         scheduleDictionary);
					var absDiff = distributionDiff(totalDistribution, distribution);
					if (absDiff > teamBlockInfoDistributionValue)
					{
						teamBlockInfoDistributionValue = absDiff;
						teamBlockInfoToWorkWith = teamBlockInfo;
					}
				}
				if (teamBlockInfoToWorkWith == null)
					continue;

				teamBlockInfoList.Remove(teamBlockInfoToWorkWith);
				var possibleTeamBlocksToSwapWith = _filterOnSwapableTeamBlocks.Filter(teamBlockInfoList, teamBlockInfoToWorkWith);
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

		private double distributionDiff(IDistributionSummary totalDistribution,
									 IDistributionSummary distributionToCalculate)
		{
			var absDiff = 0d;
			foreach (var i in distributionToCalculate.PercentDicionary)
			{
				absDiff += Math.Abs(i.Value - totalDistribution.PercentDicionary[i.Key]);
			}

			return absDiff;
		}

	}
}