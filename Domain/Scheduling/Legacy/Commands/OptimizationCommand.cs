using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class OptimizationCommand : IOptimizationCommand
	{
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ITeamBlockOptimizationCommand _teamBlockOptimizationCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IPersonListExtractorFromScheduleParts _personExtractor;
		private readonly ScheduleMatrixOriginalStateContainerCreator _scheduleMatrixOriginalStateContainerCreator;

		public OptimizationCommand(Func<IPersonSkillProvider> personSkillProvider, IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			ITeamBlockOptimizationCommand teamBlockOptimizationCommand,
			IMatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			PeriodExtractorFromScheduleParts periodExtractor,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			IPersonListExtractorFromScheduleParts personExtractor,
			ScheduleMatrixOriginalStateContainerCreator scheduleMatrixOriginalStateContainerCreator)
		{
			_personSkillProvider = personSkillProvider;
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockOptimizationCommand = teamBlockOptimizationCommand;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_periodExtractor = periodExtractor;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_personExtractor = personExtractor;
			_scheduleMatrixOriginalStateContainerCreator = scheduleMatrixOriginalStateContainerCreator;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, ISchedulingProgress backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IGroupPagePerDateHolder groupPagePerDateHolder, IScheduleOptimizerHelper scheduleOptimizerHelper,
			IOptimizationPreferences optimizationPreferences, bool optimizationMethodBackToLegalState,
			IDaysOffPreferences daysOffPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			setThreadCulture();
			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, false);
			}
			var selectedSchedules = selectedScheduleDays;
			var selectedPeriod = _periodExtractor.ExtractPeriod(selectedSchedules);
			if (!selectedPeriod.HasValue) return;

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

			var groupPersonGroupPagePerDate = _groupPageCreator.CreateGroupPagePerDate(groupPagePeriod.DayCollection(),
				_groupScheduleGroupPageDataProvider, selectedGroupPage);

			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var stateHolder = schedulerStateHolder.SchedulingResultState;

			var minutesPerInterval = 15;

			if (stateHolder.Skills.Any())
			{
				minutesPerInterval = stateHolder.Skills.Min(s => s.DefaultResolution);
			}

			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(stateHolder.Schedules);
			using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
			{
				IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();
				switch (optimizationMethodBackToLegalState)
				{

					case true:
						var scheduleMatrixOriginalStateContainers =
							_scheduleMatrixOriginalStateContainerCreator.CreateScheduleMatrixOriginalStateContainers(selectedSchedules,
								selectedPeriod.Value);
						IList<IDayOffTemplate> displayList = schedulerStateHolder.CommonStateHolder.ActiveDayOffs.ToList();
						scheduleOptimizerHelper.DaysOffBackToLegalState(scheduleMatrixOriginalStateContainers,
							backgroundWorker, displayList[0], false,
							optimizerOriginalPreferences.SchedulingOptions,
							dayOffOptimizationPreferenceProvider);

						_resourceOptimizationHelperExtended().ResourceCalculateMarkedDays(null,
							optimizerOriginalPreferences.SchedulingOptions.ConsiderShortBreaks, false);
						IList<IScheduleMatrixPro> matrixList = _matrixListFactory.CreateMatrixListForSelection(selectedSchedules);


						if (optimizationPreferences.Extra.UseTeams)
						{
							allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(selectedPeriod.Value);
						}

						scheduleOptimizerHelper.GetBackToLegalState(matrixList, schedulerStateHolder, backgroundWorker,
							optimizerOriginalPreferences.SchedulingOptions, selectedPeriod.Value,
							allMatrixes);
						break;
					case false:
						var selectedPersons = _personExtractor.ExtractPersons(selectedSchedules);
						var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
							schedulingOptions.ConsiderShortBreaks);
						var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
						var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder,
							_scheduleDayChangeCallback,
							tagSetter);

						if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
						{
							_teamBlockOptimizationCommand.Execute(backgroundWorker, selectedPeriod.Value, selectedPersons,
								optimizationPreferences,
								rollbackService, tagSetter, schedulingOptions, resourceCalculateDelayer, selectedSchedules,
								dayOffOptimizationPreferenceProvider);

							break;
						}

						groupPagePerDateHolder.GroupPersonGroupPagePerDate = groupPersonGroupPagePerDate;
						scheduleOptimizerHelper.ReOptimize(backgroundWorker, selectedSchedules, schedulingOptions,
							dayOffOptimizationPreferenceProvider);

						allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(selectedPeriod.Value);
						runWeeklyRestSolver(optimizationPreferences, schedulingOptions, selectedPeriod.Value, allMatrixes, selectedPersons,
							rollbackService, resourceCalculateDelayer, backgroundWorker, dayOffOptimizationPreferenceProvider);

						break;
				}
			}
		
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private void runWeeklyRestSolver(IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, 
										IList<IScheduleMatrixPro> allMatrixes, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, 
										IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingProgress backgroundWorker, 
										IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var singleAgentEntry = GroupPageLight.SingleAgentGroup(String.Empty);
			optimizationPreferences.Extra.TeamGroupPage = singleAgentEntry;
			optimizationPreferences.Extra.BlockTypeValue = BlockFinderType.SingleDay;
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService, resourceCalculateDelayer, 
											selectedPeriod, allMatrixes, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}
		
		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
		}
	}
}