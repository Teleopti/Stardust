using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IGroupPersonsBuilder
	{
		// Handle here when a group can't be scheduled because of different shiftCategories
        IList<IGroupPerson> BuildListOfGroupPersons(DateOnly dateOnly, IList<IPerson> selectedPersons, bool checkShiftCategoryConsistency, ISchedulingOptions schedulingOptions);
	}
}