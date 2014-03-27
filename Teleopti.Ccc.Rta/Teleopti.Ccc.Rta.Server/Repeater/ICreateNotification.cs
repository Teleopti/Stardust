using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Rta.Server.Repeater
{
	public interface ICreateNotification
	{
		Notification FromActualAgentState(IActualAgentState actualAgentState);
	}
}