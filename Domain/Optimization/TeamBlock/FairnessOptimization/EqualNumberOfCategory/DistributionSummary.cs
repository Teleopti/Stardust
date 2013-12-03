

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IDistributionSummary
	{
		IDictionary<IShiftCategory, double> PercentDicionary { get; }
	}

	public class DistributionSummary : IDistributionSummary
	{
		private readonly IDictionary<IShiftCategory, double> _percentDicionary = new Dictionary<IShiftCategory, double>();

		public DistributionSummary(Dictionary<IShiftCategory, int> distribution)
		{
			var sumOfCategories = 0;
			foreach (var keyValurPair in distribution)
			{
				sumOfCategories += keyValurPair.Value;
			}

			foreach (var keyValurPair in distribution)
			{
				var percentOfTotal = (double)keyValurPair.Value/sumOfCategories;
				PercentDicionary.Add(keyValurPair.Key, percentOfTotal);
			}
		}

		public IDictionary<IShiftCategory, double> PercentDicionary
		{
			get { return _percentDicionary; }
		}
	}
}