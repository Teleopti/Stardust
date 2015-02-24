using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TeamBlockMoveTimeBetweenDaysCommand : ITeamBlockMoveTimeBetweenDaysCommand
	{
		private readonly IScheduleCommandToggle _toggleManager;
		private IBackgroundWorkerWrapper _backgroundWorker;
		private readonly ITeamBlockMoveTimeBetweenDaysService _teamBlockMoveTimeBetweenDaysService;

		public TeamBlockMoveTimeBetweenDaysCommand(IScheduleCommandToggle toggleManager, ITeamBlockMoveTimeBetweenDaysService teamBlockMoveTimeBetweenDaysService)
		{
			_toggleManager = toggleManager;
			_teamBlockMoveTimeBetweenDaysService = teamBlockMoveTimeBetweenDaysService;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, IBackgroundWorkerWrapper backgroundWorker, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> matrixesOnSelectedperiod)
		{
			_backgroundWorker = backgroundWorker;
			if (!_toggleManager.IsEnabled(Toggles.Scheduler_TeamBlockMoveTimeBetweenDays_22407))
				return;
			_teamBlockMoveTimeBetweenDaysService.ReportProgress += moveTimePerformed;
			_teamBlockMoveTimeBetweenDaysService.Execute(optimizationPreferences, allVisibleMatrixes, rollbackService, periodValueCalculator, schedulingResultStateHolder, selectedPersons, selectedPeriod, resourceCalculateDelayer);
			_teamBlockMoveTimeBetweenDaysService.ReportProgress -= moveTimePerformed;
		}

		private void moveTimePerformed(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
				e.UserCancel = true;
			}
			_backgroundWorker.ReportProgress(1, e);
		}
	}
}