using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IShiftCategoryFairnessAggregator
	{
		IShiftCategoryFairness GetShiftCategoryFairnessForPersons(IScheduleDictionary scheduleDictionary, IList<IPerson> persons);
	}

	public class ShiftCategoryFairnessAggregator : IShiftCategoryFairnessAggregator
	{
		public IShiftCategoryFairness GetShiftCategoryFairnessForPersons(IScheduleDictionary scheduleDictionary, IList<IPerson> persons)
		{
			IShiftCategoryFairness result = null;
			foreach (var person in persons)
			{
				var schedule = scheduleDictionary[person];
				if (result == null)
					result = schedule.CachedShiftCategoryFairness();
				else
					result = result.Add(schedule.CachedShiftCategoryFairness());
			}
			return result;
		}
	}
}