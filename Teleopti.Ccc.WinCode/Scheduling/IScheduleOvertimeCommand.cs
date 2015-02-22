using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IScheduleOvertimeCommand
	{
		void Exectue(IOvertimePreferences overtimePreferences, IBackgroundWorkerWrapper backgroundWorker, IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer, IGridlockManager gridlockManager);
	}
}