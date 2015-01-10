using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public interface IActualAgentStateUpdater
	{
		void Update(StateInfo info);
	}
}