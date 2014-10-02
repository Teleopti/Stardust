using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IActualAgentStateHasBeenSent
	{
		void Invoke(IActualAgentState actualAgentState);
	}
}
