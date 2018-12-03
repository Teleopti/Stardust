using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface ILogObjectDateChecker
	{
		bool HistoricalDataIsEarlierThan(DateOnly date);
	}
}