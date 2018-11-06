using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class SchedulingScreenState
	{
		public SchedulingScreenState(ISchedulerStateHolder schedulerStateHolder)
		{
			SchedulerStateHolder = schedulerStateHolder;
		}
			
		public ISchedulerStateHolder SchedulerStateHolder { get; }
	}
}