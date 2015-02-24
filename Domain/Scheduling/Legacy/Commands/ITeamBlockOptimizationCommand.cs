using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ITeamBlockOptimizationCommand
	{
		void Execute(IBackgroundWorkerWrapper backgroundWorker, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			IScheduleTagSetter tagSetter, ISchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer, IList<IScheduleDay> selectedSchedules);
	}
}