using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ITeamBlockScheduleCommand
	{
		IWorkShiftFinderResultHolder Execute(SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker, IList<IPerson> selectedPersons,
			IEnumerable<IScheduleDay> selectedSchedules, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}