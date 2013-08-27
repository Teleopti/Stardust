using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
	public interface IShiftCategoryFairnessAggregator
	{
		IShiftCategoryFairnessHolder GetShiftCategoryFairnessForPersons(IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons);
	}

	public class ShiftCategoryFairnessAggregator : IShiftCategoryFairnessAggregator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IShiftCategoryFairnessHolder GetShiftCategoryFairnessForPersons(IScheduleDictionary scheduleDictionary, IEnumerable<IPerson> persons)
		{
			IShiftCategoryFairnessHolder result = new ShiftCategoryFairnessHolder();
			return persons.Select(person => scheduleDictionary[person]).Aggregate(result, (current, schedule) => current.Add(schedule.CachedShiftCategoryFairness()));
		}
	}
}