using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IOvertimeActivityBelongsToDateProvider
	{
		DateOnly GetBelongsToDate(IPerson person, DateTimePeriod overtimeActivityPeriod);
	}
}