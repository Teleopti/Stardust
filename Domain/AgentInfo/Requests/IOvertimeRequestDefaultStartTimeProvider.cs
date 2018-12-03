using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IOvertimeRequestDefaultStartTimeProvider
	{
		OvertimeDefaultStartTimeResult GetDefaultStartTime(DateOnly date);
	}
}