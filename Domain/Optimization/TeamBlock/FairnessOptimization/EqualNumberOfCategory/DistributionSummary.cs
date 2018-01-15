using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IDistributionSummary
	{
		IDictionary<IShiftCategory, double> PercentDicionary { get; }
	}

	public class DistributionSummary : IDistributionSummary
	{
		public DistributionSummary(Dictionary<IShiftCategory, int> distribution)
		{
			var sumOfCategories = 0;
			foreach (var keyValurPair in distribution)
			{
				sumOfCategories += keyValurPair.Value;
			}

			PercentDicionary = distribution.ToDictionary(k => k.Key, v => (double) v.Value / sumOfCategories);
		}

		public IDictionary<IShiftCategory, double> PercentDicionary { get; }
	}
}