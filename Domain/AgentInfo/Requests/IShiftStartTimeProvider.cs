using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IShiftStartTimeProvider
	{
		DateTime? GetShiftStartTimeForPerson(IPerson agent, DateOnly date);
	}
}