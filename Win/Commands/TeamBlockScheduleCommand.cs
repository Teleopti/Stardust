using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
	public interface ITeamBlockScheduleCommand
	{
		void Execute(ISchedulingOptions schedulingOptions, BackgroundWorker backgroundWorker, IList<IPerson> selectedPersons,
		             IList<IScheduleDay> selectedSchedules, ISchedulePartModifyAndRollbackService rollbackService,
		             IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class TeamBlockScheduleCommand : ITeamBlockScheduleCommand
	{
		private readonly IFixedStaffSchedulingService _fixedStaffSchedulingService;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly IAdvanceDaysOffSchedulingService _advanceDaysOffSchedulingService;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private readonly ITeamBlockMaxSeatChecker _teamBlockMaxSeatChecker;
		private BackgroundWorker _backgroundWorker;
		private int _scheduledCount;
		private ISchedulingOptions _schedulingOptions;
	    private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private readonly ITeamBlockScheduler _teamBlockScheduler;

		public TeamBlockScheduleCommand(IFixedStaffSchedulingService fixedStaffSchedulingService,
			ISchedulerStateHolder schedulerStateHolder,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			IAdvanceDaysOffSchedulingService advanceDaysOffSchedulingService,
			IMatrixListFactory matrixListFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator,
			ITeamBlockMaxSeatChecker teamBlockMaxSeatChecker,
 			ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
			ITeamBlockSchedulingCompletionChecker teamBlockSchedulingCompletionChecker,
			ITeamBlockScheduler teamBlockScheduler)
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
		}

		public void Execute(ISchedulingOptions schedulingOptions, BackgroundWorker backgroundWorker, IList<IPerson> selectedPersons, IList<IScheduleDay> selectedSchedules,
			ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			_schedulingOptions = schedulingOptions;
			_backgroundWorker = backgroundWorker;
			_fixedStaffSchedulingService.ClearFinderResults();
			if (schedulingOptions != null)
			{
				ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff =
					new SchedulePartModifyAndRollbackService(_schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback,
															 new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

				var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);

				var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedSchedules);

				IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixList(selectedSchedules, selectedPeriod);
				if (matrixesOfSelectedScheduleDays.Count == 0)
					return;

				var allVisibleMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);

				_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;
				_advanceDaysOffSchedulingService.Execute(allVisibleMatrixes, selectedPersons,
				                                         schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions,
				                                         groupPersonBuilderForOptimization);
				_advanceDaysOffSchedulingService.DayScheduled += schedulingServiceDayScheduled;

				var advanceSchedulingService = createSchedulingService(schedulingOptions, groupPersonBuilderForOptimization);

				advanceSchedulingService.DayScheduled += schedulingServiceDayScheduled;
				advanceSchedulingService.ScheduleSelected(allVisibleMatrixes, selectedPeriod,
				                                          matrixesOfSelectedScheduleDays.Select(x => x.Person).Distinct().ToList(),
				                                          rollbackService, resourceCalculateDelayer,
				                                          _schedulerStateHolder.SchedulingResultState);
				advanceSchedulingService.DayScheduled -= schedulingServiceDayScheduled;
			}
		}

		private void schedulingServiceDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			if(e.IsSuccessful)
			_scheduledCount++;
			if (_scheduledCount >= _schedulingOptions.RefreshRate)
			{
				_backgroundWorker.ReportProgress(1, e.SchedulePart);
				_scheduledCount = 0;
			}
		}

		private TeamBlockSchedulingService createSchedulingService(ISchedulingOptions schedulingOptions, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
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
											 _workShiftMinMaxCalculator, _teamBlockMaxSeatChecker,validatedTeamBlockExtractor);

			return schedulingService;
		}
	}
}