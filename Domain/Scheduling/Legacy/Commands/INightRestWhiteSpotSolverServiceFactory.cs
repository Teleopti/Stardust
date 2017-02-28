using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface INightRestWhiteSpotSolverServiceFactory
	{
		INightRestWhiteSpotSolverService Create(bool considerShortBreaks);
	}
}