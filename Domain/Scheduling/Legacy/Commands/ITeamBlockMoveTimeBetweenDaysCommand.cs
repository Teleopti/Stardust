using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ITeamBlockMoveTimeBetweenDaysCommand
	{
		void Execute(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleMatrixPro> allVisibleMatrixes, ISchedulingProgress backgroundWorker, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IScheduleMatrixPro> matrixesOnSelectedperiod);
	}
}