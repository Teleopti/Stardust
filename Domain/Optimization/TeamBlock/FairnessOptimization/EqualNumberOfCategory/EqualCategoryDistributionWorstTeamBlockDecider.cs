

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
		private readonly IEqualCategoryDistributionValue _equalCategoryDistributionValue;

		public EqualCategoryDistributionWorstTeamBlockDecider(IEqualCategoryDistributionValue equalCategoryDistributionValue)
		{
			_equalCategoryDistributionValue = equalCategoryDistributionValue;
		}

		public ITeamBlockInfo FindBlockToWorkWith(IDistributionSummary totalDistribution, IList<ITeamBlockInfo> availableTeamBlocks, IScheduleDictionary scheduleDictionary)
		{
			ITeamBlockInfo teamBlockInfoToWorkWith = null;
			var teamBlockInfoDistributionValue = 0d;
			foreach (var teamBlockInfo in availableTeamBlocks)
			{
				var absDiff = _equalCategoryDistributionValue.CalculateValue(teamBlockInfo, totalDistribution, scheduleDictionary);
				if (absDiff > teamBlockInfoDistributionValue)
				{
					teamBlockInfoDistributionValue = absDiff;
					teamBlockInfoToWorkWith = teamBlockInfo;
				}
			}

			return teamBlockInfoToWorkWith;
		}
	}
}