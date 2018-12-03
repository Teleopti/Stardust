using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopPeopleInOrganization : IAllStaff
	{
		private readonly DesktopContext _desktopContext;

		public DesktopPeopleInOrganization(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			var stateHolder = _desktopContext.CurrentContext().SchedulerStateHolderFrom.SchedulingResultState;
			return stateHolder.LoadedAgents
				.Union(stateHolder.ExternalStaff.Select(x => x.CreateExternalAgent()));
		}
	}
}