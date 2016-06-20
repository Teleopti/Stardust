using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;

		public WeeklyRestSolverCommand(ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions, Func<IWeeklyRestSolverService> weeklyRestSolverService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_weeklyRestSolverService = weeklyRestSolverService;
			_schedulerStateHolder = schedulerStateHolder;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public void Execute(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, 
						IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, ISchedulingProgress backgroundWorker,
						IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{

			_groupPersonBuilderWrapper.Reset();
			var groupPageType = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type;
			if (groupPageType == GroupPageType.SingleAgent)
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			else
				_groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
			
			var teamInfoFactory = new TeamInfoFactory(_groupPersonBuilderWrapper);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,
				_teamBlockSchedulingOptions);

			var schedulerStateHolder = _schedulerStateHolder();

			IDisposable contextDisposal = null;
			if (!ResourceCalculationContext.InContext)
			{
				contextDisposal = _resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills);
			}
			
				EventHandler<ResourceOptimizerProgressEventArgs> onResolvingWeek = (sender, e) =>
				{
					e.Cancel = backgroundWorker.CancellationPending;
					backgroundWorker.ReportProgress(1, e);
				};
				var weeklyRestSolverService = _weeklyRestSolverService();
				weeklyRestSolverService.ResolvingWeek += onResolvingWeek;
				weeklyRestSolverService.Execute(selectedPersons, selectedPeriod, teamBlockGenerator,
					rollbackService, resourceCalculateDelayer, schedulerStateHolder.SchedulingResultState, allVisibleMatrixes,
					optimizationPreferences, schedulingOptions, dayOffOptimizationPreferenceProvider);
				weeklyRestSolverService.ResolvingWeek -= onResolvingWeek;

			if (contextDisposal != null)
			{
				contextDisposal.Dispose();
			}
		}
	}
}