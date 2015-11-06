using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IWeeklyRestSolverCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, 
			IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, 
			IList<IScheduleMatrixPro> allVisibleMatrixes, IBackgroundWorkerWrapper backgroundWorker, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}