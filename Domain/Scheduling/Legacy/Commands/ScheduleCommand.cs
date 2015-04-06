using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleCommand
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ITeamBlockScheduleCommand _teamBlockScheduleCommand;
		private readonly ClassicScheduleCommand _classicScheduleCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IIntraIntervalFinderService _intraIntervalFinderService;
		private readonly IOptimizerHelperHelper _optimizerHelper;

		public ScheduleCommand(IPersonSkillProvider personSkillProvider, IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			ITeamBlockScheduleCommand teamBlockScheduleCommand,
			ClassicScheduleCommand classicScheduleCommand,
			IMatrixListFactory matrixListFactory,
			IIntraIntervalFinderService intraIntervalFinderService,
			IOptimizerHelperHelper optimizerHelper)
		{
			_personSkillProvider = personSkillProvider;
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockScheduleCommand = teamBlockScheduleCommand;
			_classicScheduleCommand = classicScheduleCommand;
			_matrixListFactory = matrixListFactory;
			_intraIntervalFinderService = intraIntervalFinderService;
			_optimizerHelper = optimizerHelper;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IGroupPagePerDateHolder groupPagePerDateHolder, IScheduleOptimizerHelper scheduleOptimizerHelper,
			IOptimizationPreferences optimizationPreferences)
		{
			setThreadCulture();
			var schedulingOptions = optimizerOriginalPreferences.SchedulingOptions;
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
			{
				var optimizationHelperWin = new ResourceOptimizationHelperWin(schedulerStateHolder, _personSkillProvider, _intraIntervalFinderService);
				optimizationHelperWin.ResourceCalculateAllDays(backgroundWorker, true);
			}

			//set to false for first scheduling and then use it for RemoveShiftCategoryBackToLegalState
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;

			var selectedPersons = selectedScheduleDays.Select(x => x.Person).Distinct().ToList();

			DateOnlyPeriod groupPagePeriod = schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
			groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate = _groupPageCreator
				.CreateGroupPagePerDate(groupPagePeriod.DayCollection(), _groupScheduleGroupPageDataProvider,
					optimizerOriginalPreferences.SchedulingOptions.GroupPageForShiftCategoryFairness);

			if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
			{
				schedulingOptions.OnlyShiftsWhenUnderstaffed = false;

				var minutesPerInterval = 15;
				if (schedulerStateHolder.SchedulingResultState.Skills.Count > 0)
				{
					minutesPerInterval = schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
				}
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval);
				var resources = extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules);
				using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
				{
					if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
					{
						var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
							schedulingOptions.ConsiderShortBreaks);

						ISchedulePartModifyAndRollbackService rollbackService =
							new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
								_scheduleDayChangeCallback,
								new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
						scheduleOptimizerHelper.WorkShiftFinderResultHolder =
							_teamBlockScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedPersons, selectedScheduleDays,
								rollbackService, resourceCalculateDelayer);
					}
					else
					{
						_classicScheduleCommand.Execute(schedulingOptions, backgroundWorker, scheduleOptimizerHelper, selectedScheduleDays,
							schedulerStateHolder);
					}
				}
			}
			else
			{
				scheduleOptimizerHelper.ScheduleSelectedStudents(selectedScheduleDays, backgroundWorker, schedulingOptions);
			}

			//shiftcategorylimitations
			if (!backgroundWorker.CancellationPending)
			{
				schedulingOptions.UseShiftCategoryLimitations = useShiftCategoryLimitations;
				if (schedulingOptions.UseShiftCategoryLimitations)
				{
					IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();
					var selectedPeriod = _optimizerHelper.GetSelectedPeriod(selectedScheduleDays);

					if (schedulingOptions.UseTeam)
					{
						allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
					}

					IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays =
						_matrixListFactory.CreateMatrixList(selectedScheduleDays, selectedPeriod);
					if (matrixesOfSelectedScheduleDays.Count == 0)
						return;

					scheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays, backgroundWorker,
						optimizationPreferences,
						schedulingOptions,
						selectedPeriod, allMatrixes);
				}
			}
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
		}
	}
}