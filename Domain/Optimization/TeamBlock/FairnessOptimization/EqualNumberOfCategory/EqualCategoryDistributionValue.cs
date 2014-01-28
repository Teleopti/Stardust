

using System;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IEqualCategoryDistributionValue
	{
		double CalculateValue(ITeamBlockInfo teamBlockInfo, IDistributionSummary totalDistribution, IScheduleDictionary scheduleDictionary);
	}

	public class EqualCategoryDistributionValue : IEqualCategoryDistributionValue
	{
		private readonly IDistributionForPersons _distributionForPersons;

		public EqualCategoryDistributionValue(IDistributionForPersons distributionForPersons)
		 {
			 _distributionForPersons = distributionForPersons;
		 }

		public double CalculateValue(ITeamBlockInfo teamBlockInfo, IDistributionSummary totalDistribution, IScheduleDictionary scheduleDictionary)
		{
			var distribution = _distributionForPersons.CreateSummary(teamBlockInfo.TeamInfo.GroupMembers,
																		 scheduleDictionary);
			var absDiff = distributionDiff(totalDistribution, distribution);

			return absDiff;
		}

		private double distributionDiff(IDistributionSummary totalDistribution,
									 IDistributionSummary distributionToCalculate)
		{
			var absDiff = 0d;
			foreach (var i in totalDistribution.PercentDicionary)
			{
				double value;
				distributionToCalculate.PercentDicionary.TryGetValue(i.Key, out value);

				absDiff += Math.Abs(value - totalDistribution.PercentDicionary[i.Key]);
			}

			return absDiff;
		}
	}
}