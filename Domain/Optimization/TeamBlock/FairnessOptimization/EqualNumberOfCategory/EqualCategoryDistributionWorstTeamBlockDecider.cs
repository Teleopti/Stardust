

using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualCategoryDistributionWorstTeamBlockDecider
	{
		ITeamBlockInfo FindBlockToWorkWith(IDistributionSummary totalDistribution, IList<ITeamBlockInfo> availableTeamBlocks, IScheduleDictionary scheduleDictionary);
	}

	public class EqualCategoryDistributionWorstTeamBlockDecider : IEqualCategoryDistributionWorstTeamBlockDecider
	{
		private readonly IDistributionForPersons _distributionForPersons;

		public EqualCategoryDistributionWorstTeamBlockDecider(IDistributionForPersons distributionForPersons)
		{
			_distributionForPersons = distributionForPersons;
		}

		public ITeamBlockInfo FindBlockToWorkWith(IDistributionSummary totalDistribution, IList<ITeamBlockInfo> availableTeamBlocks, IScheduleDictionary scheduleDictionary)
		{
			ITeamBlockInfo teamBlockInfoToWorkWith = null;
			var teamBlockInfoDistributionValue = 0d;
			foreach (var teamBlockInfo in availableTeamBlocks)
			{
				var distribution = _distributionForPersons.CreateSummary(teamBlockInfo.TeamInfo.GroupMembers,
																		 scheduleDictionary);
				var absDiff = distributionDiff(totalDistribution, distribution);
				if (absDiff > teamBlockInfoDistributionValue)
				{
					teamBlockInfoDistributionValue = absDiff;
					teamBlockInfoToWorkWith = teamBlockInfo;
				}
			}

			return teamBlockInfoToWorkWith;
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