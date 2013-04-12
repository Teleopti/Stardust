﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons);
		void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e);
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

        
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool ScheduleTeamBlockDay(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {

            var suggestedShiftProjectionCache = scheduleFirstTeamBlockToGetProjectionCache(teamBlockInfo, teamBlockInfo.BlockInfo.BlockPeriod.StartDate ,
                                                                                       schedulingOptions);
            if (suggestedShiftProjectionCache == null) return false; 
            //need to refactor the code alot i dont like these ifs probably split it into classes
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
            {
                if (!selectedPeriod.DayCollection().Contains(day)) continue;
                if (schedulingOptions.UseTeamBlockSameShift)
                {
					scheduleSelectedBlockForSameShift(teamBlockInfo, schedulingOptions, selectedPersons, day,
													  suggestedShiftProjectionCache, selectedPeriod);
                }
                else
                {
                    if (!scheduleSelectedBlock(teamBlockInfo, schedulingOptions, selectedPersons, day, suggestedShiftProjectionCache,selectedPeriod)) return false;    
                }
                
            }


            return true;
        }

	    private bool scheduleSelectedBlock(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IList<IPerson> selectedPersons, DateOnly day, IShiftProjectionCache suggestedShiftProjectionCache, DateOnlyPeriod selectedPeriod)
	    {
	        var dailyTeamBlockInfo = new TeamBlockInfo(teamBlockInfo.TeamInfo, new BlockInfo(new DateOnlyPeriod(day, day)));

            if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(dailyTeamBlockInfo, day)) return true;

	        var restriction = _restrictionAggregator.AggregatePerDay(dailyTeamBlockInfo, schedulingOptions,
	                                                                 suggestedShiftProjectionCache);

	        var shifts = _workShiftFilterService.Filter(day, dailyTeamBlockInfo, restriction,
	                                                    schedulingOptions,
	                                                    new WorkShiftFinderResult(dailyTeamBlockInfo.TeamInfo.GroupPerson, day));
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
				_teamScheduling.DayScheduled += OnDayScheduled;
	            var skipOffset = !schedulingOptions.UseTeamBlockSameShift;
                _teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache, skipOffset, selectedPeriod);
				_teamScheduling.DayScheduled -= OnDayScheduled;
	        }
	        return true;
	    }

        private bool scheduleSelectedBlockForSameShift(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IList<IPerson> selectedPersons, DateOnly day, IShiftProjectionCache suggestedShiftProjectionCache, DateOnlyPeriod selectedPeriod)
        {
            var dailyTeamBlockInfo = new TeamBlockInfo(teamBlockInfo.TeamInfo, new BlockInfo(new DateOnlyPeriod(day, day)));

            if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock( dailyTeamBlockInfo, day)) return true;

            foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
            {
                if (!selectedPersons.Contains(person)) continue;
				_teamScheduling.DayScheduled += OnDayScheduled;
                var skipOffset = !schedulingOptions.UseTeamBlockSameShift;
                _teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, suggestedShiftProjectionCache, skipOffset, selectedPeriod);
				_teamScheduling.DayScheduled -= OnDayScheduled;
            }
            return true;
        }


	    private IShiftProjectionCache scheduleFirstTeamBlockToGetProjectionCache(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions)
        {
            if (teamBlockInfo == null) return null;
            
            var restriction = _restrictionAggregator.Aggregate(teamBlockInfo, schedulingOptions);

            // (should we cover for max seats here?) 
            var shifts = _workShiftFilterService.Filter(datePointer, teamBlockInfo, restriction,
                                                        schedulingOptions, new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupPerson, datePointer));
            if (shifts == null || shifts.Count <= 0)
                return null;

            var activityInternalData = _skillDayPeriodIntervalDataGenerator.GeneratePerDay(teamBlockInfo);

            var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
                                                                                         schedulingOptions
                                                                                             .WorkShiftLengthHintOption,
                                                                                         schedulingOptions
                                                                                             .UseMinimumPersons,
                                                                                         schedulingOptions
                                                                                             .UseMaximumPersons);

            return bestShiftProjectionCache;
        }

        
        //private static bool isTeamBlockScheduled(ITeamBlockInfo teamBlockInfo,DateOnly dateOnly)
        //{
        //    IScheduleRange rangeForPerson = null;
        //    foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroup())
        //    {
        //        rangeForPerson = matrix.SchedulingStateHolder.Schedules[matrix.Person];
        //        break;
        //    }
        //    if (rangeForPerson == null) return false;
        //    //foreach (var day in selectedPeriod.DayCollection())
        //    //{
        //        IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(dateOnly);
        //        if (!scheduleDay.IsScheduled())
        //            return false;
        //    //}
            
        //    return true;
        //}

        public void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
		}
	}
}