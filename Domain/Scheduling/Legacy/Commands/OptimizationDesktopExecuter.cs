using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	//legacy class - to be used from fat client only
	public class OptimizationDesktopExecuter
	{
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private readonly IGroupPageCreator _groupPageCreator;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly TeamBlockDesktopOptimization _teamBlockOptimization;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly ScheduleOptimizerHelper _scheduleOptimizerHelper;

		public OptimizationDesktopExecuter(CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory,
			IGroupPageCreator groupPageCreator,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider,
			IResourceCalculation resourceCalculation,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			TeamBlockDesktopOptimization teamBlockOptimization,
			IUserTimeZone userTimeZone,
			IGroupPagePerDateHolder groupPagePerDateHolder,
			ScheduleOptimizerHelper scheduleOptimizerHelper)
		{
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_groupPageCreator = groupPageCreator;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
			_resourceCalculation = resourceCalculation;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockOptimization = teamBlockOptimization;
			_userTimeZone = userTimeZone;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_scheduleOptimizerHelper = scheduleOptimizerHelper;
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

			if (!schedulerStateHolder.SchedulingResultState.SkillStaffPeriodHolder.GuessResourceCalculationHasBeenMade())
			{
				using (_cascadingResourceCalculationContextFactory.Create(schedulerStateHolder.SchedulingResultState, false, selectedPeriod))
				{
					_resourceCalculation.ResourceCalculate(selectedPeriod, new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, false, optimizationPreferences.General.OptimizationStepIntraInterval));
				}			
			}

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
					schedulerStateHolder.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(_userTimeZone.TimeZone()).DayCollection(), _groupScheduleGroupPageDataProvider, optimizationPreferences.Extra.TeamGroupPage);
				_groupPagePerDateHolder.GroupPersonGroupPagePerDate = groupPersonGroupPagePerDate;
				_scheduleOptimizerHelper.ReOptimize(backgroundWorker, selectedAgents, selectedPeriod, schedulingOptions,
					dayOffOptimizationPreferenceProvider, optimizationPreferences, resourceCalculateDelayer, rollbackService);
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}
	}
}