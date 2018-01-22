using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IOvertimeRequestDefaultStartTimeProvider
	{
		OvertimeDefaultStartTimeResult GetDefaultStartTime(DateOnly date);
	}
}