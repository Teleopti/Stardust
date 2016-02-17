using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ITeamBlockMoveTimeBetweenDaysCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, ISchedulingProgress backgroundWorker, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> matrixesOnSelectedperiod);
	}
}