using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public interface IDesktopContextData
	{
		ISchedulerStateHolder SchedulerStateHolderFrom { get; }
	}
}