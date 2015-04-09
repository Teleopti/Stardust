using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class WeeklyRestSolverCommand : IWeeklyRestSolverCommand
	{
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly Func<IWeeklyRestSolverService> _weeklyRestSolverService;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly IScheduleCommandToggle _toggleManager;
		private IBackgroundWorkerWrapper _backgroundWorker;

		public WeeklyRestSolverCommand(ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions, Func<IWeeklyRestSolverService> weeklyRestSolverService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory, IScheduleCommandToggle toggleManager)
		{
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_weeklyRestSolverService = weeklyRestSolverService;
			_schedulerStateHolder = schedulerStateHolder;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_toggleManager = toggleManager;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, IBackgroundWorkerWrapper backgroundWorker)
		{
			if (!_toggleManager.IsEnabled(Toggles.Scheduler_WeeklyRestRuleSolver_27108))
				return;

			_backgroundWorker = backgroundWorker;
			var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
			var teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,
				_teamBlockSchedulingOptions);

			EventHandler<ResourceOptimizerProgressEventArgs> onResolvingWeek = (sender, e) =>
			{
				e.Cancel = _backgroundWorker.CancellationPending;
				_backgroundWorker.ReportProgress(1, e);
			};
			var weeklyRestSolverService = _weeklyRestSolverService();
			weeklyRestSolverService.ResolvingWeek += onResolvingWeek;
			weeklyRestSolverService.Execute(selectedPersons, selectedPeriod, teamBlockGenerator,
				rollbackService, resourceCalculateDelayer, _schedulerStateHolder().SchedulingResultState, allVisibleMatrixes,
				optimizationPreferences, schedulingOptions);
			weeklyRestSolverService.ResolvingWeek -= onResolvingWeek;
		}
	}
}