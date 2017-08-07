using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopPeopleInOrganization : IPeopleInOrganization
	{
		private readonly DesktopContext _desktopContext;

		public DesktopPeopleInOrganization(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			return _desktopContext.CurrentContext().SchedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization;
		}
	}
}