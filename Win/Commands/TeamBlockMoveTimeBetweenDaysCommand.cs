using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public interface ITeamBlockMoveTimeBetweenDaysCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, BackgroundWorker backgroundWorker, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> matrixesOnSelectedperiod);
	}

	public class TeamBlockMoveTimeBetweenDaysCommand : ITeamBlockMoveTimeBetweenDaysCommand
	{
		private readonly IToggleManager _toggleManager;
		private BackgroundWorker _backgroundWorker;
		private readonly ITeamBlockMoveTimeBetweenDaysService  _teamBlockMoveTimeBetweenDaysService;

		public TeamBlockMoveTimeBetweenDaysCommand(IToggleManager toggleManager, ITeamBlockMoveTimeBetweenDaysService teamBlockMoveTimeBetweenDaysService)
		{
			_toggleManager = toggleManager;
			_teamBlockMoveTimeBetweenDaysService = teamBlockMoveTimeBetweenDaysService;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, BackgroundWorker backgroundWorker, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> matrixesOnSelectedperiod)
		{
			_backgroundWorker = backgroundWorker;
			if (!_toggleManager.IsEnabled(Toggles.TeamBlockMoveTimeBetweenDays))
				return;
			_teamBlockMoveTimeBetweenDaysService.ReportProgress  += moveTimePerformed;
			_teamBlockMoveTimeBetweenDaysService.Execute(optimizationPreferences, allVisibleMatrixes, rollbackService, periodValueCalculator, schedulingResultStateHolder, selectedPersons, matrixesOnSelectedperiod);
			_teamBlockMoveTimeBetweenDaysService.ReportProgress  -= moveTimePerformed;
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
