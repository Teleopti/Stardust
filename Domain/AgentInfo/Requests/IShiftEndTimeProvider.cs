using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IShiftEndTimeProvider
	{
		DateTime? GetShiftEndTimeForPerson(IPerson person, DateOnly date);
	}
}