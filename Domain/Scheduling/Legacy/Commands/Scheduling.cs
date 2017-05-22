using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class Scheduling : IScheduling
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly Func<IWorkShiftFinderResultHolder> _workShiftFinderResultHolder;
		private readonly AdvanceDaysOffSchedulingService _advanceDaysOffSchedulingService;
		private readonly MatrixListFactory _matrixListFactory;
		private ISchedulingProgress _backgroundWorker;
		private int _scheduledCount;
		private SchedulingOptions _schedulingOptions;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IUserTimeZone _userTimeZone;
		private readonly TeamBlockSchedulingService _teamBlockSchedulingService;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;

		public Scheduling(Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			AdvanceDaysOffSchedulingService advanceDaysOffSchedulingService,
			MatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IResourceCalculation resourceCalculation,
			IUserTimeZone userTimeZone,
			TeamBlockSchedulingService teamBlockSchedulingService,
			TeamInfoFactoryFactory teamInfoFactoryFactory)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
			_advanceDaysOffSchedulingService = advanceDaysOffSchedulingService;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_resourceCalculation = resourceCalculation;
			_userTimeZone = userTimeZone;
			_teamBlockSchedulingService = teamBlockSchedulingService;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
		}

		[RemoveMeWithToggle("Remove param backgroundworker", Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_workShiftFinderResultHolder().Clear();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ResourceCalculateFrequency,
				schedulingOptions.ConsiderShortBreaks, _schedulerStateHolder().SchedulingResultState, _userTimeZone);
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

			_schedulingOptions = schedulingOptions;
			_backgroundWorker = backgroundWorker;

			var schedulePartModifyAndRollbackServiceForContractDaysOff =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

			var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().AllPermittedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);

			//can we use fewer also with team? what happens if AgentA and AgentB have different scheduleperiods? (does it even matter?)
			var matrixes = schedulingOptions.UseTeam ? 
				_matrixListFactory.CreateMatrixListAllForLoadedPeriod(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization, selectedPeriod) : 
				_matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedAgents, selectedPeriod);


			_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			_advanceDaysOffSchedulingService.Execute(matrixes, selectedAgents,
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions,
				_groupPersonBuilderWrapper, selectedPeriod);
			_advanceDaysOffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

			_teamBlockSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			var workShiftFinderResultHolder = _teamBlockSchedulingService.ScheduleSelected(matrixes, selectedPeriod,
				selectedAgents, rollbackService, resourceCalculateDelayer,
				_schedulerStateHolder().SchedulingResultState, schedulingOptions,
				teamInfoFactory);
			_teamBlockSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

			_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedAgents, rollbackService, resourceCalculateDelayer,
				selectedPeriod, matrixes, _backgroundWorker, dayOffOptimizationPreferenceProvider);

			_workShiftFinderResultHolder()
				.AddResults(workShiftFinderResultHolder.GetResults(), DateTime.Today);
		}

		private void schedulingServiceDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			if (e.IsSuccessful)
				_scheduledCount++;
			if (_scheduledCount >= _schedulingOptions.RefreshRate)
			{
				_backgroundWorker.ReportProgress(1, e);
				_scheduledCount = 0;
			}
		}
	}
}