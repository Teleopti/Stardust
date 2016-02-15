using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.IslandScheduling
{
	public interface IIslandSchedulerStateHolder
	{
		void SetCommonStateHolder(ICommonStateHolder commonStateHolder);
	}
}