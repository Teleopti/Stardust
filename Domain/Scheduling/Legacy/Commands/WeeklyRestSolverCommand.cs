using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		private readonly Func<IWeeklyRestSolverService> _weeklyRestSolverService;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;

		public WeeklyRestSolverCommand(ITeamBlockInfoFactory teamBlockInfoFactory,
			Func<IWeeklyRestSolverService> weeklyRestSolverService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			TeamInfoFactoryFactory teamInfoFactoryFactory)
		{
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_weeklyRestSolverService = weeklyRestSolverService;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
		}

		[TestLog]
		public virtual void Execute(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, 
						IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allVisibleMatrixes, ISchedulingProgress backgroundWorker,
						IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().AllPermittedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory);

			var schedulerStateHolder = _schedulerStateHolder();

			IDisposable contextDisposal = null;
			if (!ResourceCalculationContext.InContext)
			{
#pragma warning disable 618
				contextDisposal = _resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, false);
#pragma warning restore 618
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

			contextDisposal?.Dispose();
		}
	}
}