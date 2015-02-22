using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface ITeamBlockScheduleCommand
	{
		IWorkShiftFinderResultHolder Execute(ISchedulingOptions schedulingOptions, IBackgroundWorkerWrapper backgroundWorker, IList<IPerson> selectedPersons,
			IList<IScheduleDay> selectedSchedules, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer);
	}
}