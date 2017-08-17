using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public interface IScheduling
	{
		void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class TeamBlockSchedulingOLD : IScheduling
	{
		private readonly IFixedStaffSchedulingService _fixedStaffSchedulingService;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly Func<IWorkShiftFinderResultHolder> _workShiftFinderResultHolder;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly AdvanceDaysOffSchedulingServiceOLD _advanceDaysOffSchedulingService;
		private readonly MatrixListFactory _matrixListFactory;
		private ISchedulingProgress _backgroundWorker;
		private int _scheduledCount;
		private SchedulingOptions _schedulingOptions;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IUserTimeZone _userTimeZone;
		private readonly TeamBlockSchedulingService _teamBlockSchedulingService;

		public TeamBlockSchedulingOLD(IFixedStaffSchedulingService fixedStaffSchedulingService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			AdvanceDaysOffSchedulingServiceOLD advanceDaysOffSchedulingService,
			MatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IResourceCalculation resourceCalculation,
			IUserTimeZone userTimeZone,
			TeamBlockSchedulingService teamBlockSchedulingService)
		{
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_advanceDaysOffSchedulingService = advanceDaysOffSchedulingService;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_resourceCalculation = resourceCalculation;
			_userTimeZone = userTimeZone;
			_teamBlockSchedulingService = teamBlockSchedulingService;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod)
		{
			_workShiftFinderResultHolder().Clear();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, 1,
				schedulingOptions.ConsiderShortBreaks, _schedulerStateHolder().SchedulingResultState, _userTimeZone);
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

			_schedulingOptions = schedulingOptions;
			_backgroundWorker = backgroundWorker;
			_fixedStaffSchedulingService.ClearFinderResults();

			var schedulePartModifyAndRollbackServiceForContractDaysOff =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

			_groupPersonBuilderWrapper.Reset();
			var groupPageType = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type;
			if (groupPageType == GroupPageType.SingleAgent)
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			else
				_groupPersonBuilderForOptimizationFactory.Create(_schedulerStateHolder().AllPermittedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);

			var matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedAgents, selectedPeriod);
			if (!matrixesOfSelectedScheduleDays.Any())
				return;

			var allVisibleMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization, selectedPeriod);

			_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			_advanceDaysOffSchedulingService.Execute(allVisibleMatrixes, selectedAgents.ToArray(),
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions,
				_groupPersonBuilderWrapper, selectedPeriod);
			_advanceDaysOffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

			var workShiftFinderResultHolder = _teamBlockSchedulingService.ScheduleSelected(schedulingCallback, allVisibleMatrixes, selectedPeriod,
				matrixesOfSelectedScheduleDays.Select(x => x.Person).Distinct().ToList(),
				rollbackService, resourceCalculateDelayer,
				_schedulerStateHolder().SchedulingResultState, schedulingOptions,
				new TeamInfoFactory(_groupPersonBuilderWrapper));

			_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedAgents.ToArray(), rollbackService, resourceCalculateDelayer,
				selectedPeriod, allVisibleMatrixes, _backgroundWorker, null);

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