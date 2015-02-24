using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class IntraIntervalOptimizationCommand : IIntraIntervalOptimizationCommand
	{
		private readonly IIntraIntervalOptimizationService _intervalOptimizationService;
		private IBackgroundWorkerWrapper _backgroundWorker;
		private string _optimizationstep;

		public IntraIntervalOptimizationCommand(IIntraIntervalOptimizationService intervalOptimizationService)
		{
			_intervalOptimizationService = intervalOptimizationService;
		}

		public void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IBackgroundWorkerWrapper backgroundWorker)
		{
			_optimizationstep = UserTexts.Resources.IntraIntervalOptimization + ": ";
			_backgroundWorker = backgroundWorker;
			_intervalOptimizationService.ReportProgress += intervalOptimizationServiceReportProgress;
			_intervalOptimizationService.Execute(schedulingOptions, selectedPeriod, selectedSchedules, schedulingResultStateHolder, allScheduleMatrixPros, rollbackService, resourceCalculateDelayer);
			_intervalOptimizationService.Execute(schedulingOptions, selectedPeriod, selectedSchedules, schedulingResultStateHolder, allScheduleMatrixPros, rollbackService, resourceCalculateDelayer);
			_intervalOptimizationService.ReportProgress -= intervalOptimizationServiceReportProgress;
		}

		void intervalOptimizationServiceReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
				e.UserCancel = true;
			}
			var args = new ResourceOptimizerProgressEventArgs(0, 0, _optimizationstep + e.Message);
			_backgroundWorker.ReportProgress(1, args);
		}
	}
}