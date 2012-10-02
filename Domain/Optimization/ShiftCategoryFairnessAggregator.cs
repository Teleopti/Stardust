using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
			IShiftCategoryFairness result = new ShiftCategoryFairness();
			return persons.Select(person => scheduleDictionary[person]).Aggregate(result, (current, schedule) => current.Add(schedule.CachedShiftCategoryFairness()));
		}
	}
}