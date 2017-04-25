using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TeamBlockScheduleCommand : ITeamBlockScheduleCommand
	{
		private readonly IFixedStaffSchedulingService _fixedStaffSchedulingService;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly IAdvanceDaysOffSchedulingService _advanceDaysOffSchedulingService;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly Func<IWorkShiftMinMaxCalculator> _workShiftMinMaxCalculator;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private ISchedulingProgress _backgroundWorker;
		private int _scheduledCount;
		private SchedulingOptions _schedulingOptions;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly ITeamMatrixChecker _teamMatrixChecker;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public TeamBlockScheduleCommand(IFixedStaffSchedulingService fixedStaffSchedulingService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			IAdvanceDaysOffSchedulingService advanceDaysOffSchedulingService,
			IMatrixListFactory matrixListFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			Func<IWorkShiftMinMaxCalculator> workShiftMinMaxCalculator,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
			ITeamBlockScheduler teamBlockScheduler, IWeeklyRestSolverCommand weeklyRestSolverCommand,
			ITeamMatrixChecker teamMatrixChecker,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			PeriodExtractorFromScheduleParts periodExtractor,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_advanceDaysOffSchedulingService = advanceDaysOffSchedulingService;
			_matrixListFactory = matrixListFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_teamBlockScheduler = teamBlockScheduler;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_teamMatrixChecker = teamMatrixChecker;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_periodExtractor = periodExtractor;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public IWorkShiftFinderResultHolder Execute(SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker,
			IList<IPerson> selectedPersons, IEnumerable<IScheduleDay> selectedSchedules,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			_schedulingOptions = schedulingOptions;
			_backgroundWorker = backgroundWorker;
			_fixedStaffSchedulingService.ClearFinderResults();
			if (schedulingOptions == null)
				return new WorkShiftFinderResultHolder();

			var schedulePartModifyAndRollbackServiceForContractDaysOff =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

			_groupPersonBuilderWrapper.Reset();
			var groupPageType = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type;
			if (groupPageType == GroupPageType.SingleAgent)
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			else
				_groupPersonBuilderForOptimizationFactory.Create(_schedulerStateHolder().AllPermittedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);

			var selectedPeriod = _periodExtractor.ExtractPeriod(selectedSchedules);

			IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedSchedules);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return new WorkShiftFinderResultHolder();

			var allVisibleMatrixes = selectedPeriod.HasValue ? _matrixListFactory.CreateMatrixListAllForLoadedPeriod(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization, selectedPeriod.Value) : new List<IScheduleMatrixPro>();

			_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			_advanceDaysOffSchedulingService.Execute(allVisibleMatrixes, selectedPersons,
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions,
				_groupPersonBuilderWrapper, selectedPeriod.GetValueOrDefault());
			_advanceDaysOffSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

			var advanceSchedulingService = createSchedulingService(schedulingOptions, _groupPersonBuilderWrapper);

			advanceSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			var workShiftFinderResultHolder = advanceSchedulingService.ScheduleSelected(allVisibleMatrixes, selectedPeriod.GetValueOrDefault(),
				matrixesOfSelectedScheduleDays.Select(x => x.Person).Distinct().ToList(),
				rollbackService, resourceCalculateDelayer,
				_schedulerStateHolder().SchedulingResultState);
			advanceSchedulingService.DayScheduled -= schedulingServiceDayScheduled;

			if (selectedPeriod.HasValue)
			{
				_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons, rollbackService, resourceCalculateDelayer,
					selectedPeriod.Value, allVisibleMatrixes, _backgroundWorker, dayOffOptimizationPreferenceProvider);
			}

			return workShiftFinderResultHolder;
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
				//_backgroundWorker.ReportProgress(1, e.SchedulePart);
				_backgroundWorker.ReportProgress(1, e);
				_scheduledCount = 0;
			}
		}

		private TeamBlockSchedulingService createSchedulingService(SchedulingOptions schedulingOptions, IGroupPersonBuilderWrapper groupPersonBuilderForOptimization)
		{
			ITeamInfoFactory teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
			IValidatedTeamBlockInfoExtractor validatedTeamBlockExtractor =
				new ValidatedTeamBlockInfoExtractor(_teamBlockSteadyStateValidator, _teamBlockInfoFactory, _teamBlockSchedulingCompletionChecker);
			var schedulingService =
				new TeamBlockSchedulingService(schedulingOptions,
					teamInfoFactory,
					_teamBlockScheduler,
					_safeRollbackAndResourceCalculation,
					_workShiftMinMaxCalculator(),
					validatedTeamBlockExtractor,
					_teamMatrixChecker,
					_workShiftSelector,
					_groupPersonSkillAggregator);

			return schedulingService;
		}
	}
}