using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IShiftEndTimeProvider
	{
		DateTime? GetShiftEndTimeForPerson(IPerson person, DateOnly date);
	}
}