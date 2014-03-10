using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
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

		public OptimizationCommand(IPersonSkillProvider personSkillProvider, IGroupPageCreator groupPageCreator,
		                           IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
		                           IResourceOptimizationHelper resourceOptimizationHelper,
		                           IScheduleDayChangeCallback scheduleDayChangeCallback,
								   ITeamBlockOptimizationCommand teamBlockOptimizationCommand,
		                           IMatrixListFactory matrixListFactory)
		{
			_personSkillProvider = personSkillProvider;
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockOptimizationCommand = teamBlockOptimizationCommand;
			_matrixListFactory = matrixListFactory;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, BackgroundWorker backgroundWorker,
		                    ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
		                    IGroupPagePerDateHolder groupPagePerDateHolder, ScheduleOptimizerHelper scheduleOptimizerHelper,
		                    IOptimizationPreferences optimizationPreferences, bool optimizationMethodBackToLegalState,
							IDaysOffPreferences daysOffPreferences)
		{
			setThreadCulture();
			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
			{
				var optimizationHelperWin = new ResourceOptimizationHelperWin(schedulerStateHolder, _personSkillProvider);
				optimizationHelperWin.ResourceCalculateAllDays(null, true);
			}
			var selectedSchedules = selectedScheduleDays;
			var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedSchedules);
			var scheduleMatrixOriginalStateContainers = scheduleOptimizerHelper.CreateScheduleMatrixOriginalStateContainers(selectedSchedules, selectedPeriod);
			DateOnlyPeriod groupPagePeriod = schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;

			IGroupPageLight selectedGroupPage;
			// ***** temporary cope
			if (optimizationMethodBackToLegalState)
			{
				selectedGroupPage = optimizerOriginalPreferences.SchedulingOptions.GroupPageForShiftCategoryFairness;
			}
			else
			{
				selectedGroupPage = optimizationPreferences.Extra.GroupPageOnTeamBlockPer;
			}

			groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate = _groupPageCreator.CreateGroupPagePerDate(groupPagePeriod.DayCollection(), _groupScheduleGroupPageDataProvider, selectedGroupPage);


			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
	
			IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();
			switch (optimizationMethodBackToLegalState)
			{

				case true:
					IList<IDayOffTemplate> displayList = schedulerStateHolder.CommonStateHolder.ActiveDayOffs.ToList();
					scheduleOptimizerHelper.DaysOffBackToLegalState(scheduleMatrixOriginalStateContainers,
																	 backgroundWorker, displayList[0], false,
																	 optimizerOriginalPreferences.SchedulingOptions,
																	 daysOffPreferences);

					var optimizationHelperWin = new ResourceOptimizationHelperWin(schedulerStateHolder, _personSkillProvider);
					optimizationHelperWin.ResourceCalculateMarkedDays(null, optimizerOriginalPreferences.SchedulingOptions.ConsiderShortBreaks, true);
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

					if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
					{
						var extractor = new PersonListExtractorFromScheduleParts(selectedSchedules);
						var selectedPersons = extractor.ExtractPersons().ToList();

						var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
																					true,
																					schedulingOptions.ConsiderShortBreaks);

						var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);

						var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
																					   _scheduleDayChangeCallback,
																					   tagSetter);


						_teamBlockOptimizationCommand.Execute(backgroundWorker, selectedPeriod, selectedPersons, optimizationPreferences,
						                                      rollbackService, tagSetter, schedulingOptions, resourceCalculateDelayer);

						break;
					}

					// we need it here for fairness opt. for example
					groupPagePerDateHolder.GroupPersonGroupPagePerDate = groupPagePerDateHolder.ShiftCategoryFairnessGroupPagePerDate;
					scheduleOptimizerHelper.ReOptimize(backgroundWorker, selectedSchedules, schedulingOptions);

					break;
			}


			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.Current.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.Current.Regional.UICulture;
		}
	}
}