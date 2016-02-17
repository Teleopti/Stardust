using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IScheduleOvertimeCommand
	{
		void Exectue(IOvertimePreferences overtimePreferences, ISchedulingProgress backgroundWorker, IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer, IGridlockManager gridlockManager);
	}
}