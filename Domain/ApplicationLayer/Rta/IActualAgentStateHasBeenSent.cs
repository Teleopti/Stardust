using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IActualAgentStateHasBeenSent
	{
		void Invoke(IActualAgentState actualAgentState);
	}
}
