using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface INightRestWhiteSpotSolverServiceFactory
	{
		INightRestWhiteSpotSolverService Create(bool considerShortBreaks);
	}
}