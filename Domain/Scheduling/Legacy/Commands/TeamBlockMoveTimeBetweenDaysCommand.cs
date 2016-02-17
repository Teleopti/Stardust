using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TeamBlockMoveTimeBetweenDaysCommand : ITeamBlockMoveTimeBetweenDaysCommand
	{
		private ISchedulingProgress _backgroundWorker;
		private readonly ITeamBlockMoveTimeBetweenDaysService _teamBlockMoveTimeBetweenDaysService;

		public TeamBlockMoveTimeBetweenDaysCommand(ITeamBlockMoveTimeBetweenDaysService teamBlockMoveTimeBetweenDaysService)
		{
			_teamBlockMoveTimeBetweenDaysService = teamBlockMoveTimeBetweenDaysService;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, ISchedulingProgress backgroundWorker, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> matrixesOnSelectedperiod)
		{
			_backgroundWorker = backgroundWorker;
			_teamBlockMoveTimeBetweenDaysService.ReportProgress += moveTimePerformed;
			_teamBlockMoveTimeBetweenDaysService.Execute(optimizationPreferences, allVisibleMatrixes, rollbackService, periodValueCalculator, schedulingResultStateHolder, selectedPersons, selectedPeriod, resourceCalculateDelayer);
			_teamBlockMoveTimeBetweenDaysService.ReportProgress -= moveTimePerformed;
		}

		private void moveTimePerformed(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			_backgroundWorker.ReportProgress(1, e);
		}
	}
}