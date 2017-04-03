using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class OptimizationExecuter
	{
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly ITeamBlockOptimizationCommand _teamBlockOptimizationCommand;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IPersonListExtractorFromScheduleParts _personExtractor;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly ScheduleOptimizerHelper _scheduleOptimizerHelper;
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		public OptimizationExecuter(IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IResourceCalculation resourceOptimizationHelper,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			ITeamBlockOptimizationCommand teamBlockOptimizationCommand,
			PeriodExtractorFromScheduleParts periodExtractor,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			IPersonListExtractorFromScheduleParts personExtractor,
			IUserTimeZone userTimeZone,
			IGroupPagePerDateHolder groupPagePerDateHolder,
			ScheduleOptimizerHelper scheduleOptimizerHelper,
			DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime)
		{
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockOptimizationCommand = teamBlockOptimizationCommand;
			_periodExtractor = periodExtractor;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_personExtractor = personExtractor;
			_userTimeZone = userTimeZone;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_scheduleOptimizerHelper = scheduleOptimizerHelper;
			_doFullResourceOptimizationOneTime = doFullResourceOptimizationOneTime;
		}

		public void Execute(ISchedulingProgress backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, 
			IList<IScheduleDay> selectedScheduleDays,
			IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var selectedPeriod = _periodExtractor.ExtractPeriod(selectedScheduleDays);
			if (!selectedPeriod.HasValue)
				return;

			var lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;

			if (lastCalculationState)
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, false);
			}
#pragma warning disable 618
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary();
#pragma warning restore 618

			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
				schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback, tagSetter);

			if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
			{
				_teamBlockOptimizationCommand.Execute(backgroundWorker, selectedPeriod.Value, _personExtractor.ExtractPersons(selectedScheduleDays),
						optimizationPreferences, rollbackService, tagSetter, schedulingOptions, resourceCalculateDelayer, selectedScheduleDays, dayOffOptimizationPreferenceProvider);
			}
			else
			{
				var groupPersonGroupPagePerDate = _groupPageCreator.CreateGroupPagePerDate(schedulerStateHolder.AllPermittedPersons, schedulerStateHolder.Schedules, 
					schedulerStateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection(), _groupScheduleGroupPageDataProvider, optimizationPreferences.Extra.TeamGroupPage);

				_groupPagePerDateHolder.GroupPersonGroupPagePerDate = groupPersonGroupPagePerDate;
				_scheduleOptimizerHelper.ReOptimize(backgroundWorker, selectedScheduleDays, schedulingOptions,
					dayOffOptimizationPreferenceProvider, optimizationPreferences, resourceCalculateDelayer, rollbackService);
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}
	}
}