

using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	public interface IDistributionForPersons
	{
		IDistributionSummary CreateSummary(IEnumerable<IPerson> personList, IScheduleDictionary scheduleDictionary);
	}

	public class DistributionForPersons : IDistributionForPersons
	{
		public IDistributionSummary CreateSummary(IEnumerable<IPerson> personList, IScheduleDictionary scheduleDictionary)
		{
			return new DistributionSummary(personList.SelectMany(person =>
					scheduleDictionary[person].CachedShiftCategoryFairness().ShiftCategoryFairnessDictionary)
				.GroupBy(k => k.Key, (v, y) => new {Category = v, Sum = y.Sum(x => x.Value)})
				.ToDictionary(g => g.Category, v => v.Sum));
		}
	}
}