using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class OptimizationCommand
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ITeamBlockOptimizationCommand _teamBlockOptimizationCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IIntraIntervalFinderService _intraIntraIntervalFinderService;
		private readonly IOptimizerHelperHelper _optimizerHelper;

		public OptimizationCommand(IPersonSkillProvider personSkillProvider, IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			ITeamBlockOptimizationCommand teamBlockOptimizationCommand,
			IMatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IIntraIntervalFinderService intraIntraIntervalFinderService,
			IOptimizerHelperHelper optimizerHelper)
		{
			_personSkillProvider = personSkillProvider;
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockOptimizationCommand = teamBlockOptimizationCommand;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_intraIntraIntervalFinderService = intraIntraIntervalFinderService;
			_optimizerHelper = optimizerHelper;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IGroupPagePerDateHolder groupPagePerDateHolder, IScheduleOptimizerHelper scheduleOptimizerHelper,
			IOptimizationPreferences optimizationPreferences, bool optimizationMethodBackToLegalState,
			IDaysOffPreferences daysOffPreferences)
		{
			setThreadCulture();
			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
			{
				var optimizationHelperWin = new ResourceOptimizationHelperWin(schedulerStateHolder, _personSkillProvider, _intraIntraIntervalFinderService);
				optimizationHelperWin.ResourceCalculateAllDays(backgroundWorker, true);
			}
			var selectedSchedules = selectedScheduleDays;
			var selectedPeriod = _optimizerHelper.GetSelectedPeriod(selectedSchedules);
			var scheduleMatrixOriginalStateContainers = scheduleOptimizerHelper.CreateScheduleMatrixOriginalStateContainers(selectedSchedules, selectedPeriod);
			DateOnlyPeriod groupPagePeriod = schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;

			IGroupPageLight selectedGroupPage;

			if (optimizationMethodBackToLegalState)
			{
				selectedGroupPage = optimizerOriginalPreferences.SchedulingOptions.GroupPageForShiftCategoryFairness;
			}
			else
			{
				selectedGroupPage = optimizationPreferences.Extra.TeamGroupPage;
			}

			groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate = _groupPageCreator.CreateGroupPagePerDate(groupPagePeriod.DayCollection(), _groupScheduleGroupPageDataProvider, selectedGroupPage);


			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var stateHolder = schedulerStateHolder.SchedulingResultState;

			IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();
			switch (optimizationMethodBackToLegalState)
			{

				case true:
					IList<IDayOffTemplate> displayList = schedulerStateHolder.CommonStateHolder.ActiveDayOffs.ToList();
					scheduleOptimizerHelper.DaysOffBackToLegalState(scheduleMatrixOriginalStateContainers,
						backgroundWorker, displayList[0], false,
						optimizerOriginalPreferences.SchedulingOptions,
						daysOffPreferences);

					var optimizationHelperWin = new ResourceOptimizationHelperWin(schedulerStateHolder, _personSkillProvider,
						_intraIntraIntervalFinderService);
					optimizationHelperWin.ResourceCalculateMarkedDays(null,
						optimizerOriginalPreferences.SchedulingOptions.ConsiderShortBreaks, true);
					IList<IScheduleMatrixPro> matrixList = _matrixListFactory.CreateMatrixList(selectedSchedules, selectedPeriod);


					if (optimizationPreferences.Extra.UseTeams)
					{
						allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
					}

					scheduleOptimizerHelper.GetBackToLegalState(matrixList, schedulerStateHolder, backgroundWorker,
						optimizerOriginalPreferences.SchedulingOptions, selectedPeriod,
						allMatrixes);
					break;
				case false:
					var personExtractor = new PersonListExtractorFromScheduleParts(selectedSchedules);
					var selectedPersons = personExtractor.ExtractPersons().ToList();
					var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
						true,
						schedulingOptions.ConsiderShortBreaks);
					var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
					var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder,
						_scheduleDayChangeCallback,
						tagSetter);

					var minutesPerInterval = 15;

					if (stateHolder.Skills.Any())
					{
						minutesPerInterval = stateHolder.Skills.Min(s => s.DefaultResolution);
					}

					var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval);
					var resources = extractor.CreateRelevantProjectionList(stateHolder.Schedules);
					using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
					{
						if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
						{
							_teamBlockOptimizationCommand.Execute(backgroundWorker, selectedPeriod, selectedPersons, optimizationPreferences,
								rollbackService, tagSetter, schedulingOptions, resourceCalculateDelayer, selectedSchedules);

							break;
						}

						// we need it here for fairness opt. for example
						groupPagePerDateHolder.GroupPersonGroupPagePerDate = groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate;
						scheduleOptimizerHelper.ReOptimize(backgroundWorker, selectedSchedules, schedulingOptions);

						allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
						runWeeklyRestSolver(optimizationPreferences, schedulingOptions, selectedPeriod, allMatrixes, selectedPersons,
							rollbackService, resourceCalculateDelayer, backgroundWorker);

						break;
					}
			}
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private void runWeeklyRestSolver(IOptimizationPreferences optimizationPreferences, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, IBackgroundWorkerWrapper backgroundWorker)
		{
			var singleAgentEntry = new GroupPageLight { Key = "SingleAgentTeam", Name = Resources.SingleAgentTeam };
			optimizationPreferences.Extra.TeamGroupPage = singleAgentEntry;
			optimizationPreferences.Extra.BlockTypeValue = BlockFinderType.SingleDay;
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService, resourceCalculateDelayer, selectedPeriod, allMatrixes, backgroundWorker);
		}


		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
		}
	}
}