using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

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
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private ISchedulingProgress _backgroundWorker;
		private int _scheduledCount;
		private ISchedulingOptions _schedulingOptions;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly ITeamMatrixChecker _teamMatrixChecker;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly PeriodExctractorFromScheduleParts _periodExctractor;

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
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
			ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
			ITeamBlockScheduler teamBlockScheduler, IWeeklyRestSolverCommand weeklyRestSolverCommand,
			ITeamMatrixChecker teamMatrixChecker,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			PeriodExctractorFromScheduleParts periodExctractor)
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
			_teamBlockMaxSeatChecker = teamBlockMaxSeatChecker;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_teamBlockSchedulingCompletionChecker = teamBlockSchedulingCompletionChecker;
			_teamBlockScheduler = teamBlockScheduler;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_teamMatrixChecker = teamMatrixChecker;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_periodExctractor = periodExctractor;
		}

		public IWorkShiftFinderResultHolder Execute(ISchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker,
			IList<IPerson> selectedPersons, IList<IScheduleDay> selectedSchedules,
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
				_groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);

			var selectedPeriod = _periodExctractor.ExtractPeriod(selectedSchedules);

			IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(selectedSchedules);
			if (matrixesOfSelectedScheduleDays.Count == 0)
				return new WorkShiftFinderResultHolder();

			var allVisibleMatrixes = selectedPeriod.HasValue ? _matrixListFactory.CreateMatrixListAllForLoadedPeriod(selectedPeriod.Value) : new List<IScheduleMatrixPro>();

			_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
			_advanceDaysOffSchedulingService.Execute(allVisibleMatrixes, selectedPersons,
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions,
				_groupPersonBuilderWrapper);
			_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;

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

		private TeamBlockSchedulingService createSchedulingService(ISchedulingOptions schedulingOptions, IGroupPersonBuilderWrapper groupPersonBuilderForOptimization)
		{
			ITeamInfoFactory teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
			IValidatedTeamBlockInfoExtractor validatedTeamBlockExtractor =
				new ValidatedTeamBlockInfoExtractor(_teamBlockSteadyStateValidator, _teamBlockInfoFactory,
					_teamBlockSchedulingOptions, _teamBlockSchedulingCompletionChecker);
			var schedulingService =
				new TeamBlockSchedulingService(schedulingOptions,
					teamInfoFactory,
					_teamBlockScheduler,
					_safeRollbackAndResourceCalculation,
					_workShiftMinMaxCalculator(),
					_teamBlockMaxSeatChecker,
					validatedTeamBlockExtractor,
					_teamMatrixChecker);

			return schedulingService;
		}
	}
}