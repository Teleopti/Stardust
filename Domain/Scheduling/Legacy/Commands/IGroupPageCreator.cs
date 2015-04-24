using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPageCreator
	{
		IGroupPagePerDate CreateGroupPagePerDate(ISelectedPeriod currentView, IGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping);
		IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping);
		IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping, bool useAllLoadedPersons);
	}
}