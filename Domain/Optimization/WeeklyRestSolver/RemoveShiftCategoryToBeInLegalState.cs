using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_RemoveClassicShiftCat_46582)]
	public class RemoveShiftCategoryToBeInLegalStateAlwaysTeamBlock : RemoveShiftCategoryToBeInLegalState
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly TeamBlockRetryRemoveShiftCategoryBackToLegalService _teamBlockRemoveShiftCategoryBackToLegalService;

		public RemoveShiftCategoryToBeInLegalStateAlwaysTeamBlock(Func<ISchedulerStateHolder> schedulerStateHolder, RequiredScheduleHelper requiredScheduleOptimizerHelper, Func<IScheduleDayChangeCallback> scheduleDayChangeCallback, MatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand, IUserTimeZone userTimeZone, IResourceCalculation resourceCalculation,
			TeamBlockRetryRemoveShiftCategoryBackToLegalService teamBlockRemoveShiftCategoryBackToLegalService) 
			: base(schedulerStateHolder, requiredScheduleOptimizerHelper, scheduleDayChangeCallback, matrixListFactory, weeklyRestSolverCommand, userTimeZone, resourceCalculation)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockRemoveShiftCategoryBackToLegalService = teamBlockRemoveShiftCategoryBackToLegalService;
		}

		protected override void RemoveShiftCat(SchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker, IEnumerable<IScheduleMatrixPro> matrixesOfSelectedScheduleDays)
		{
			_teamBlockRemoveShiftCategoryBackToLegalService.Execute(schedulingOptions, _schedulerStateHolder().SchedulingResultState, matrixesOfSelectedScheduleDays, backgroundWorker);
		}
	}
	
	public class RemoveShiftCategoryToBeInLegalState
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly RequiredScheduleHelper _requiredScheduleOptimizerHelper;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IResourceCalculation _resourceCalculation;

		public RemoveShiftCategoryToBeInLegalState(Func<ISchedulerStateHolder> schedulerStateHolder,
			RequiredScheduleHelper requiredScheduleOptimizerHelper,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			MatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IUserTimeZone userTimeZone,
			IResourceCalculation resourceCalculation)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleOptimizerHelper = requiredScheduleOptimizerHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_userTimeZone = userTimeZone;
			_resourceCalculation = resourceCalculation;
		}

		public void Execute(bool useShiftCategoryLimitations, SchedulingOptions schedulingOptions,
			IEnumerable<IPerson> selectedPersons,
			DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker)
		{
			schedulingOptions.UseShiftCategoryLimitations = useShiftCategoryLimitations;
			if (schedulingOptions.UseShiftCategoryLimitations)
			{
				var schedulerStateHolder = _schedulerStateHolder();
				var matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedPersons, selectedPeriod);
				if (!matrixesOfSelectedScheduleDays.Any())
					return;

				RemoveShiftCat(schedulingOptions, selectedPeriod, backgroundWorker, matrixesOfSelectedScheduleDays);

				var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
				_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedPersons, rollbackService,
					resourceCalculateDelayer, selectedPeriod, matrixesOfSelectedScheduleDays, backgroundWorker, null);
			}
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582)]
		protected virtual void RemoveShiftCat(SchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod,
			ISchedulingProgress backgroundWorker, IEnumerable<IScheduleMatrixPro> matrixesOfSelectedScheduleDays)
		{
			_requiredScheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays,
				backgroundWorker,
				schedulingOptions,
				selectedPeriod);
		}
	}
}