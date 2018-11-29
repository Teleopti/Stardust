using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public interface IAllStaff
	{
		IEnumerable<IPerson> Agents(DateOnlyPeriod period);
	}
}