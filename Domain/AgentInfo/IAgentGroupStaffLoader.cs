using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IAgentGroupStaffLoader
	{
		PeopleSelection Load(DateOnlyPeriod period, IAgentGroup agentGroup);
	}
}