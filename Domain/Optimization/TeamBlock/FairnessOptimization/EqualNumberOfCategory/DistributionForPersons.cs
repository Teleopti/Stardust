

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IDistributionForPersons
	{
		Dictionary<IShiftCategory, int> CreateSummary(IEnumerable<IPerson> personList, IScheduleDictionary scheduleDictionary);
	}

	public class DistributionForPersons : IDistributionForPersons
	{
		public Dictionary<IShiftCategory, int> CreateSummary(IEnumerable<IPerson> personList, IScheduleDictionary scheduleDictionary)
		{
			var totalsDictionary = new Dictionary<IShiftCategory, int>();
			foreach (var person in personList)
			{
				var fairness = scheduleDictionary[person].CachedShiftCategoryFairness();
				foreach (var categoryValue in fairness.ShiftCategoryFairnessDictionary)
				{
					var shiftCategory = categoryValue.Key;
					if (!totalsDictionary.ContainsKey(shiftCategory))
						totalsDictionary.Add(shiftCategory, 0);
					totalsDictionary[shiftCategory] += categoryValue.Value;
				}
			}

			return totalsDictionary;
		}
	}
}