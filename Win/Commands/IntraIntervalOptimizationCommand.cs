using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public interface IIntraIntervalOptimizationCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, BackgroundWorker backgroundWorker);
	}

	public class IntraIntervalOptimizationCommand : IIntraIntervalOptimizationCommand
	{
		private readonly IIntraIntervalOptimizationService _intervalOptimizationService;
		private BackgroundWorker _backgroundWorker;
		private string _optimizationstep;

		public IntraIntervalOptimizationCommand(IIntraIntervalOptimizationService intervalOptimizationService)
		{
			_intervalOptimizationService = intervalOptimizationService;
		}

		public void Execute(ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleDay> selectedSchedules, ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allScheduleMatrixPros, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, BackgroundWorker backgroundWorker)
		{
			_optimizationstep = LanguageResourceHelper.Translate("XXIntraIntervalOptimization") + ": ";
			_backgroundWorker = backgroundWorker;
			_intervalOptimizationService.ReportProgress += intervalOptimizationServiceReportProgress;
			_intervalOptimizationService.Execute(schedulingOptions,selectedPeriod,selectedSchedules,schedulingResultStateHolder,allScheduleMatrixPros,rollbackService,resourceCalculateDelayer);
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
