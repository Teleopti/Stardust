using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IOvertimeActivityBelongsToDateProvider
	{
		DateOnly GetBelongsToDate(IPerson person, DateTimePeriod overtimeActivityPeriod);
	}
}