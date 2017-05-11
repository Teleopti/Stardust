using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class IntraIntervalOptimizationCommand
	{
		private readonly IntraIntervalOptimizationService _intervalOptimizationService;
		private ISchedulingProgress _backgroundWorker;

		public IntraIntervalOptimizationCommand(IntraIntervalOptimizationService intervalOptimizationService)
		{
			_intervalOptimizationService = intervalOptimizationService;
		}

		public void Execute(IOptimizationPreferences optimizationPreferences,
			DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, 
			IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, 
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingProgress backgroundWorker)
		{
			_backgroundWorker = backgroundWorker;
			_intervalOptimizationService.ReportProgress += intervalOptimizationServiceReportProgress;
			_intervalOptimizationService.Execute(optimizationPreferences, selectedPeriod, selectedSchedules, schedulingResultStateHolder, allScheduleMatrixPros, rollbackService, resourceCalculateDelayer);
			_intervalOptimizationService.Execute(optimizationPreferences, selectedPeriod, selectedSchedules, schedulingResultStateHolder, allScheduleMatrixPros, rollbackService, resourceCalculateDelayer);
			_intervalOptimizationService.ReportProgress -= intervalOptimizationServiceReportProgress;
		}

		void intervalOptimizationServiceReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			var args = new ResourceOptimizerProgressEventArgs(0, 0, UserTexts.Resources.IntraIntervalOptimization + ": " + e.Message, e.ScreenRefreshRate);
			_backgroundWorker.ReportProgress(1, args);
		}
	}
}