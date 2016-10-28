using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPageCreator
	{
		IGroupPagePerDate CreateGroupPagePerDate(IEnumerable<IPerson> allPermittedPersons, IEnumerable<DateOnly> dates, IGroupScheduleGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping);
	}
}