using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IGroupPersonShiftCategoryConsistentChecker
	{
		bool AllPersonsHasSameOrNoneShiftCategoryScheduled(IScheduleDictionary scheduleDictionary, IList<IPerson> persons, DateOnly dateOnly);
		IShiftCategory CommonShiftCategory { get; }
	}
}