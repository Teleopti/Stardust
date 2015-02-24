using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ISelectedPeriod
	{
		DateOnlyPeriod Period();
	}
}