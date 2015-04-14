using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IIntraIntervalOptimizationCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IBackgroundWorkerWrapper backgroundWorker);
	}
}