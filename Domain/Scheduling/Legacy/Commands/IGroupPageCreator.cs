using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPageCreator
	{
		IGroupPagePerDate CreateGroupPagePerDate(IEnumerable<IPerson> allPermittedPersons, IScheduleDictionary schedules, IEnumerable<DateOnly> dates, IGroupScheduleGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping);
	}
}