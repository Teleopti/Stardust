using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IAfterSend
	{
		void Invoke(IActualAgentState actualAgentState);
	}
}
