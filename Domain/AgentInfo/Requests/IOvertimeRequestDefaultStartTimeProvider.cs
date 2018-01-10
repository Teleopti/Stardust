using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IOvertimeRequestDefaultStartTimeProvider
	{
		DateTime GetDefaultStartTime(DateOnly date);
	}
}