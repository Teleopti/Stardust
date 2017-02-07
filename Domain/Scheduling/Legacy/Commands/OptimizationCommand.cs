using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class OptimizationCommand : IOptimizationCommand
	{
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ITeamBlockOptimizationCommand _teamBlockOptimizationCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IPersonListExtractorFromScheduleParts _personExtractor;
		private readonly ScheduleMatrixOriginalStateContainerCreator _scheduleMatrixOriginalStateContainerCreator;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly IRequiredScheduleHelper _requiredScheduleHelper;
		private readonly ScheduleOptimizerHelper _scheduleOptimizerHelper;
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		public OptimizationCommand(IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IResourceCalculation resourceOptimizationHelper,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			ITeamBlockOptimizationCommand teamBlockOptimizationCommand,
			IMatrixListFactory matrixListFactory,
			PeriodExtractorFromScheduleParts periodExtractor,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			IPersonListExtractorFromScheduleParts personExtractor,
			ScheduleMatrixOriginalStateContainerCreator scheduleMatrixOriginalStateContainerCreator,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IUserTimeZone userTimeZone,
			IGroupPagePerDateHolder groupPagePerDateHolder,
			WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
			IRequiredScheduleHelper requiredScheduleHelper,
			ScheduleOptimizerHelper scheduleOptimizerHelper,
			DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime)
		{
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockOptimizationCommand = teamBlockOptimizationCommand;
			_matrixListFactory = matrixListFactory;
			_periodExtractor = periodExtractor;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_personExtractor = personExtractor;
			_scheduleMatrixOriginalStateContainerCreator = scheduleMatrixOriginalStateContainerCreator;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_userTimeZone = userTimeZone;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_requiredScheduleHelper = requiredScheduleHelper;
			_scheduleOptimizerHelper = scheduleOptimizerHelper;
			_doFullResourceOptimizationOneTime = doFullResourceOptimizationOneTime;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, ISchedulingProgress backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IOptimizationPreferences optimizationPreferences, bool optimizationMethodBackToLegalState,
			IDaysOffPreferences daysOffPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var selectedSchedules = selectedScheduleDays;
			var selectedPeriod = _periodExtractor.ExtractPeriod(selectedSchedules);
			if (!selectedPeriod.HasValue) return;

			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;

			if (lastCalculationState)
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, false);
			}
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary();

			DateOnlyPeriod groupPagePeriod = schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;

			GroupPageLight selectedGroupPage;

			if (optimizationMethodBackToLegalState)
			{
				selectedGroupPage = optimizerOriginalPreferences.SchedulingOptions.GroupOnGroupPageForTeamBlockPer;
			}
			else
			{
				selectedGroupPage = optimizationPreferences.Extra.TeamGroupPage;
			}

			var groupPersonGroupPagePerDate = _groupPageCreator.CreateGroupPagePerDate(schedulerStateHolder.AllPermittedPersons, schedulerStateHolder.Schedules, groupPagePeriod.DayCollection(), _groupScheduleGroupPageDataProvider, selectedGroupPage);

			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);

			if (optimizationMethodBackToLegalState)
			{
#pragma warning disable 618
				using (_resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, false))
#pragma warning restore 618
				{
					var scheduleMatrixOriginalStateContainers =
						_scheduleMatrixOriginalStateContainerCreator.CreateScheduleMatrixOriginalStateContainers(schedulerStateHolder.Schedules, selectedSchedules, selectedPeriod.Value);
					IList<IDayOffTemplate> displayList = schedulerStateHolder.CommonStateHolder.ActiveDayOffs.ToList();
					_scheduleOptimizerHelper.DaysOffBackToLegalState(scheduleMatrixOriginalStateContainers,
						backgroundWorker, displayList[0], optimizerOriginalPreferences.SchedulingOptions,
						dayOffOptimizationPreferenceProvider, optimizationPreferences);

					_resourceOptimizationHelperExtended().ResourceCalculateMarkedDays(null,
						optimizerOriginalPreferences.SchedulingOptions.ConsiderShortBreaks, false);
					IList<IScheduleMatrixPro> matrixList = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedSchedules);

					var allMatrixes = optimizationPreferences.Extra.UseTeams ? 
						_matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.PersonsInOrganization, selectedPeriod.Value) : 
						new List<IScheduleMatrixPro>();

					backToLegalState(matrixList, schedulerStateHolder, optimizationPreferences, backgroundWorker,
						optimizerOriginalPreferences.SchedulingOptions, selectedPeriod.Value,
						allMatrixes);
				}
			}
			else
			{
				var selectedPersons = _personExtractor.ExtractPersons(selectedSchedules);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
					schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
				var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
				var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
					_scheduleDayChangeCallback,
					tagSetter);

				if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
				{
					_teamBlockOptimizationCommand.Execute(backgroundWorker, selectedPeriod.Value, selectedPersons,
							optimizationPreferences,
							rollbackService, tagSetter, schedulingOptions, resourceCalculateDelayer, selectedSchedules,
							dayOffOptimizationPreferenceProvider);
				}
				else
				{
					_groupPagePerDateHolder.GroupPersonGroupPagePerDate = groupPersonGroupPagePerDate;
					_scheduleOptimizerHelper.ReOptimize(backgroundWorker, selectedSchedules, schedulingOptions,
						dayOffOptimizationPreferenceProvider, optimizationPreferences, resourceCalculateDelayer, rollbackService);
				}
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private void backToLegalState(IList<IScheduleMatrixPro> matrixList,
				ISchedulerStateHolder schedulerStateHolder,
				IOptimizationPreferences optimizationPreferences,
				ISchedulingProgress backgroundWorker,
				ISchedulingOptions schedulingOptions,
				DateOnlyPeriod selectedPeriod,
				IList<IScheduleMatrixPro> allMatrixes)
		{
			if (matrixList == null) throw new ArgumentNullException("matrixList");
			if (schedulerStateHolder == null) throw new ArgumentNullException("schedulerStateHolder");
			if (backgroundWorker == null) throw new ArgumentNullException("backgroundWorker");
			foreach (var scheduleMatrix in matrixList)
			{
				var schedulePartModifyAndRollbackService =
					new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback,
						new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
				var workShiftBackToLegalStateServicePro = _workShiftBackToLegalStateServiceProFactory.Create();
				workShiftBackToLegalStateServicePro.Execute(scheduleMatrix, schedulingOptions, schedulePartModifyAndRollbackService);

				backgroundWorker.ReportProgress(1);
			}

			if (optimizationPreferences.General.UseShiftCategoryLimitations)
			{
				_requiredScheduleHelper.RemoveShiftCategoryBackToLegalState(matrixList, backgroundWorker, optimizationPreferences,
					schedulingOptions,
					selectedPeriod, allMatrixes);
			}
		}
	}
}