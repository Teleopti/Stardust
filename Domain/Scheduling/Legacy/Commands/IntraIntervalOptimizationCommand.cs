using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class IntraIntervalOptimizationCommand
	{
		private readonly IntraIntervalOptimizationService _intervalOptimizationService;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private ISchedulingProgress _backgroundWorker;

		public IntraIntervalOptimizationCommand(IntraIntervalOptimizationService intervalOptimizationService, MatrixListFactory matrixListFactory, Func<ISchedulerStateHolder> stateHolder)
		{
			_intervalOptimizationService = intervalOptimizationService;
			_matrixListFactory = matrixListFactory;
			_stateHolder = stateHolder;
		}

		public void Execute(IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedAgents, 
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingProgress backgroundWorker)
		{
			_backgroundWorker = backgroundWorker;
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_stateHolder().Schedules, _stateHolder().SchedulingResultState.LoadedAgents, selectedPeriod);
			new MatrixOvertimeLocker(allMatrixes).Execute();
			_intervalOptimizationService.ReportProgress += intervalOptimizationServiceReportProgress;
			_intervalOptimizationService.Execute(optimizationPreferences, selectedPeriod, selectedAgents, _stateHolder().SchedulingResultState, allMatrixes, rollbackService, resourceCalculateDelayer);
			_intervalOptimizationService.Execute(optimizationPreferences, selectedPeriod, selectedAgents, _stateHolder().SchedulingResultState, allMatrixes, rollbackService, resourceCalculateDelayer);
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