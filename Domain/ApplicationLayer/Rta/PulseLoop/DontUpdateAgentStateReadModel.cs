using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop
{
	public class DontUpdateAgentStateReadModel : IAgentStateReadModelUpdater
	{
		public void Update(StateInfo info)
		{
		}
	}
}