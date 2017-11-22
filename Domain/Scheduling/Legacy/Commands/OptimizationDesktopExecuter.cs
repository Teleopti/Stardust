using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public class OptimizationDesktopExecuterNew : OptimizationDesktopExecuter
	{
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private readonly IResourceCalculation _resourceCalculation;

		public OptimizationDesktopExecuterNew(CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory, IGroupPageCreator groupPageCreator, IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider, IResourceCalculation resourceCalculation, IScheduleDayChangeCallback scheduleDayChangeCallback, TeamBlockDesktopOptimization teamBlockOptimization, Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended, IUserTimeZone userTimeZone, IGroupPagePerDateHolder groupPagePerDateHolder, ScheduleOptimizerHelper scheduleOptimizerHelper, DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime) 
			: base(groupPageCreator, groupScheduleGroupPageDataProvider, resourceCalculation, scheduleDayChangeCallback, teamBlockOptimization, resourceOptimizationHelperExtended, userTimeZone, groupPagePerDateHolder, scheduleOptimizerHelper, doFullResourceOptimizationOneTime)
		{
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_resourceCalculation = resourceCalculation;
		}

		protected override void PreOptimize(ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod, ISchedulingProgress backgroundWorker, bool lastCalculationState, bool doIntraIntervalCalculation)
		{
			if (!schedulerStateHolder.SchedulingResultState.GuessResourceCalculationHasBeenMade())
			{
				using (_cascadingResourceCalculationContextFactory.Create(schedulerStateHolder.SchedulingResultState, false, selectedPeriod))
				{
					_resourceCalculation.ResourceCalculate(selectedPeriod, new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, false, doIntraIntervalCalculation));
				}			
			}
		}
	}
	
	
	//legacy class - to be used from fat client only
	public class OptimizationDesktopExecuter
	{
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly TeamBlockDesktopOptimization _teamBlockOptimization;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly ScheduleOptimizerHelper _scheduleOptimizerHelper;
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		[RemoveMeWithToggle("Remove unnecessary params", Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
		public OptimizationDesktopExecuter(IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IResourceCalculation resourceCalculation,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			TeamBlockDesktopOptimization teamBlockOptimization,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			IUserTimeZone userTimeZone,
			IGroupPagePerDateHolder groupPagePerDateHolder,
			ScheduleOptimizerHelper scheduleOptimizerHelper,
			DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime)
		{
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceCalculation = resourceCalculation;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockOptimization = teamBlockOptimization;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_userTimeZone = userTimeZone;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_scheduleOptimizerHelper = scheduleOptimizerHelper;
			_doFullResourceOptimizationOneTime = doFullResourceOptimizationOneTime;
		}

		public void Execute(ISchedulingProgress backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, 
			IEnumerable<IPerson> selectedAgents,
			DateOnlyPeriod selectedPeriod, 
			IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;

			PreOptimize(schedulerStateHolder, selectedPeriod, backgroundWorker, lastCalculationState, optimizationPreferences.General.OptimizationStepIntraInterval);

			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation,
				schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback, tagSetter);

			if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
			{
				_teamBlockOptimization.Execute(backgroundWorker, selectedPeriod, selectedAgents,
						optimizationPreferences, rollbackService, tagSetter, schedulingOptions, resourceCalculateDelayer, dayOffOptimizationPreferenceProvider);
			}
			else
			{
				var groupPersonGroupPagePerDate = _groupPageCreator.CreateGroupPagePerDate(schedulerStateHolder.ChoosenAgents, schedulerStateHolder.Schedules, 
					schedulerStateHolder.RequestedPeriod.DateOnlyPeriod.DayCollection(), _groupScheduleGroupPageDataProvider, optimizationPreferences.Extra.TeamGroupPage);

				_groupPagePerDateHolder.GroupPersonGroupPagePerDate = groupPersonGroupPagePerDate;
				_scheduleOptimizerHelper.ReOptimize(backgroundWorker, selectedAgents, selectedPeriod, schedulingOptions,
					dayOffOptimizationPreferenceProvider, optimizationPreferences, resourceCalculateDelayer, rollbackService);
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
		protected virtual void PreOptimize(ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod selectedPeriod, ISchedulingProgress backgroundWorker, bool lastCalculationState, bool doIntraIntervalCalculation)
		{
			if (lastCalculationState)
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, false);
			}
#pragma warning disable 618
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary();
#pragma warning restore 618
		}
	}
}