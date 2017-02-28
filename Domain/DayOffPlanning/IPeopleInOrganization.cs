using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public interface IPeopleInOrganization
	{
		IEnumerable<IPerson> Agents(DateOnlyPeriod period);
	}
}