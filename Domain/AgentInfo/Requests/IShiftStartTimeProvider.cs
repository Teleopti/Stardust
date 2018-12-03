using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IShiftStartTimeProvider
	{
		DateTime? GetShiftStartTimeForPerson(IPerson agent, DateOnly date);
	}
}