using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	//remove me when cascading is used everywhere (and not only for overtime)
	public interface IResourceCalculateDelayerFactory
	{
		IResourceCalculateDelayer Create(int calculationFrequency, bool considerShortBreaks);
	}
}