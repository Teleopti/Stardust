using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public interface IActualAgentStateHasBeenSent
	{
		void Invoke(IActualAgentState actualAgentState);
	}
}
