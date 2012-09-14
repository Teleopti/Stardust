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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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