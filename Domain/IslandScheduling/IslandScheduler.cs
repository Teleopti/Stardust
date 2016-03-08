using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.IslandScheduling
{
	public class IslandScheduler
	{
		private readonly Func<IScheduleCommand> _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IRequiredScheduleHelper> _requiredScheduleHelper;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;

		public IslandScheduler(Func<IScheduleCommand> scheduleCommand, Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IRequiredScheduleHelper> requiredScheduleHelper, Func<IGroupPagePerDateHolder> groupPagePerDateHolder)
		{
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleHelper = requiredScheduleHelper;
			_groupPagePerDateHolder = groupPagePerDateHolder;
		}

		public IScheduleDictionary RunIsland(SkillGroupIslandsAnalyzer.Island island, IOptimizerOriginalPreferences optimizerOriginalPreferences,
			IList<IScheduleDay> selectedScheduleDays, IOptimizationPreferences optimizationPreferences, ISchedulerStateHolder schedulingScreenStateHolder)
		{
			var dayOffOptimizePreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences());
			var scheduleCommand = _scheduleCommand();
			var schedulerStateHolder = _schedulerStateHolder();
			var requiredScheduleHelper = _requiredScheduleHelper();
			var groupPagePerDateHolder = _groupPagePerDateHolder();

			var personsInIsland = island.PersonsInIsland();
			var x = new IslandSchedulerStateProvider(schedulingScreenStateHolder, personsInIsland, island.SkillGuidStrings);
			schedulerStateHolder = x.Load(schedulerStateHolder);
			var islandSelectedScheduleDays = new List<IScheduleDay>();
			foreach (var selectedScheduleDay in selectedScheduleDays)
			{
				if(personsInIsland.Contains(selectedScheduleDay.Person))
					islandSelectedScheduleDays.Add(selectedScheduleDay);
			}

			scheduleCommand
				.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), schedulerStateHolder, islandSelectedScheduleDays,
					groupPagePerDateHolder, requiredScheduleHelper, new OptimizationPreferences(), false,
					dayOffOptimizePreferenceProvider);

			return schedulerStateHolder.Schedules;
		} 
	}
}