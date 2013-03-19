using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons);
        bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions);
		bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, bool skipOffset);
	}

	public class TeamBlockScheduler : ITeamBlockScheduler
	{
		private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private readonly IRestrictionAggregator _restrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ITeamScheduling _teamScheduling;
		private readonly IWorkShiftSelector _workShiftSelector;

		public TeamBlockScheduler(ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
			IRestrictionAggregator restrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			ITeamScheduling teamScheduling,
			IWorkShiftSelector workShiftSelector)
		{
			_skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
			_restrictionAggregator = restrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_teamScheduling = teamScheduling;
			_workShiftSelector = workShiftSelector;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        public bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions)
		{
			return ScheduleTeamBlock(teamBlockInfo, datePointer, schedulingOptions, false);
		}

		public bool ScheduleTeamBlock(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, bool skipOffset)
		{
            if (teamBlockInfo == null)
				return false;

			//if teamBlockInfo is fully scheduled, continue;
		    if (isTeamBlockScheduled(teamBlockInfo)) return false;

			var restriction = _restrictionAggregator.Aggregate(teamBlockInfo,schedulingOptions);

			// (should we cover for max seats here?) ????
			var shifts = _workShiftFilterService.Filter(datePointer, teamBlockInfo, restriction,
														schedulingOptions, new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupPerson, datePointer));
			if (shifts == null || shifts.Count <= 0)
				return false;

			var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(teamBlockInfo);

            var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																						 schedulingOptions
																							 .WorkShiftLengthHintOption,
																						 schedulingOptions
																							 .UseMinimumPersons,
																						 schedulingOptions
																							 .UseMaximumPersons);
			//implement
			_teamScheduling.DayScheduled += dayScheduled;
			_teamScheduling.Execute(teamBlockInfo, bestShiftProjectionCache, skipOffset);
			_teamScheduling.DayScheduled -= dayScheduled;

			return true;
		}

        public bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {

           var suggestedShiftProjectionCache = ScheduleFirstTeamBlockToGetProjectionCache(teamBlockInfo, datePointer,
                                                                                       schedulingOptions);
            if (suggestedShiftProjectionCache == null) return false; 

            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
            {
                if (!selectedPeriod.DayCollection().Contains(day)) continue;
                var dailyTeamBlockInfo = new TeamBlockInfo(teamBlockInfo.TeamInfo, new BlockInfo(new DateOnlyPeriod(day, day)));

                if (isTeamBlockScheduled(dailyTeamBlockInfo)) continue;
                //should pass the suggested shift here

                var restriction = _restrictionAggregator.AggregatePerDay(dailyTeamBlockInfo, schedulingOptions, suggestedShiftProjectionCache);

                //should consider the suggested start time
                var shifts = _workShiftFilterService.Filter(day, dailyTeamBlockInfo, restriction,
                                                            schedulingOptions, new WorkShiftFinderResult(dailyTeamBlockInfo.TeamInfo.GroupPerson, day));
                if (shifts == null || shifts.Count <= 0)
                    return false;
                
                foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
                {
                    if (!selectedPersons.Contains(person)) continue;
                    var activityInternalData = _skillDayPeriodIntervalDataGenerator.GeneratePerDay(dailyTeamBlockInfo);
                    var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
                                                                                                 schedulingOptions
                                                                                                     .WorkShiftLengthHintOption,
                                                                                                 schedulingOptions
                                                                                                     .UseMinimumPersons,
                                                                                                 schedulingOptions
                                                                                                     .UseMaximumPersons);
                    _teamScheduling.DayScheduled += dayScheduled;
                    var skipOffset = !schedulingOptions.UseLevellingSameShift;
                    _teamScheduling.ExecutePerDayPerPerson(person,day,teamBlockInfo,bestShiftProjectionCache,skipOffset);
                    _teamScheduling.DayScheduled -= dayScheduled;
                }
                
            }

           

            return true;
        }

        private IShiftProjectionCache ScheduleFirstTeamBlockToGetProjectionCache(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions)
        {
            if (teamBlockInfo == null) return null;
            
            var restriction = _restrictionAggregator.Aggregate(teamBlockInfo, schedulingOptions);

            // (should we cover for max seats here?) ????
            var shifts = _workShiftFilterService.Filter(datePointer, teamBlockInfo, restriction,
                                                        schedulingOptions, new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupPerson, datePointer));
            if (shifts == null || shifts.Count <= 0)
                return null;

            var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(teamBlockInfo);

            var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
                                                                                         schedulingOptions
                                                                                             .WorkShiftLengthHintOption,
                                                                                         schedulingOptions
                                                                                             .UseMinimumPersons,
                                                                                         schedulingOptions
                                                                                             .UseMaximumPersons);

            return bestShiftProjectionCache;
        }

        private bool isTeamBlockScheduled(ITeamBlockInfo teamBlockInfo)
        {
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    if (!matrix.GetScheduleDayByKey(day).DaySchedulePart().IsScheduled())
                        return false;
            return true;
        }

        void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			OnDayScheduled(e);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
		}
	}
}