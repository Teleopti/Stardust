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
		private readonly Func<WeeklyRestSolverService> _weeklyRestSolverService;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly BlockPreferencesMapper _blockPreferencesMapper;

		public WeeklyRestSolverCommand(ITeamBlockInfoFactory teamBlockInfoFactory,
			Func<WeeklyRestSolverService> weeklyRestSolverService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			TeamInfoFactoryFactory teamInfoFactoryFactory, 
			BlockPreferencesMapper blockPreferencesMapper)
		{
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_weeklyRestSolverService = weeklyRestSolverService;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_blockPreferencesMapper = blockPreferencesMapper;
		}

		[TestLog]
		public virtual void Execute(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEnumerable<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, 
						IResourceCalculateDelayer resourceCalculateDelayer, DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleMatrixPro> allVisibleMatrixes, ISchedulingProgress backgroundWorker,
						IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().ChoosenAgents, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory, _blockPreferencesMapper);

			var schedulerStateHolder = _schedulerStateHolder();

			IDisposable contextDisposal = null;
			if (!ResourceCalculationContext.InContext)
			{
				contextDisposal = _resourceCalculationContextFactory.Create(schedulerStateHolder, false, selectedPeriod.Inflate(1));
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