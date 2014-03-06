using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IAfterSend
	{
		void AfterSend(IActualAgentState actualAgentState);
	}
}
