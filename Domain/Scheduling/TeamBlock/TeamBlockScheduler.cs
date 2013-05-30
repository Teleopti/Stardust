using System;
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
		private bool _cancelMe;

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
            if (teamBlockInfo == null) throw new ArgumentNullException("teamBlockInfo");
			var suggestedShiftProjectionCache = scheduleFirstTeamBlockToGetProjectionCache(teamBlockInfo, datePointer,
                                                                                       schedulingOptions);
            if (suggestedShiftProjectionCache == null && !(schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseGroupScheduling)) 
				return false; 

            //need to refactor the code alot i dont like these ifs probably split it into classes
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
            {
	            if (_cancelMe)
		            return false;
                if (!selectedPeriod.DayCollection().Contains(day)) 
					continue;
                if (schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameShift)
                {
	                if (!scheduleSelectedBlockForSameShift(teamBlockInfo, schedulingOptions, selectedPersons, day,
	                                                       suggestedShiftProjectionCache, selectedPeriod))
		                return false;
					if (schedulingOptions.UseGroupScheduling && schedulingOptions.UseTeamBlockPerOption)
		                suggestedShiftProjectionCache = null;
                }
                else
                {
                    if (!scheduleSelectedBlock(teamBlockInfo, schedulingOptions, selectedPersons, day, suggestedShiftProjectionCache,selectedPeriod)) 
						return false;    
					suggestedShiftProjectionCache = null;
                }
            }
            return true;
        }

	    private bool scheduleSelectedBlock(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IList<IPerson> selectedPersons, DateOnly day, IShiftProjectionCache suggestedShiftProjectionCache, DateOnlyPeriod selectedPeriod)
	    {
	        var dailyTeamBlockInfo = new TeamBlockInfo(teamBlockInfo.TeamInfo, new BlockInfo(new DateOnlyPeriod(day, day)));

            if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(dailyTeamBlockInfo, day)) return true;
		    var roleModelShift = suggestedShiftProjectionCache;
			var isTeamScheduling = false;
	        foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
	        {
				if (_cancelMe)
					return false;
	            if (!selectedPersons.Contains(person)) continue;
		        
		        var restriction = _restrictionAggregator.AggregatePerDayPerPerson(day, person, teamBlockInfo,
		                                                                          schedulingOptions,
																				  suggestedShiftProjectionCache, isTeamScheduling);
				var shifts = _workShiftFilterService.Filter(day,person, dailyTeamBlockInfo, restriction, roleModelShift, schedulingOptions,
															new WorkShiftFinderResult(dailyTeamBlockInfo.TeamInfo.GroupPerson, day));
				if (shifts == null || shifts.Count <= 0)
					continue;
	            var activityInternalData = _skillDayPeriodIntervalDataGenerator.GeneratePerDay(dailyTeamBlockInfo);
	            var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
	                                                                                         schedulingOptions
	                                                                                             .WorkShiftLengthHintOption,
	                                                                                         schedulingOptions
	                                                                                             .UseMinimumPersons,
	                                                                                         schedulingOptions
	                                                                                             .UseMaximumPersons);
				if (roleModelShift == null)
					roleModelShift = bestShiftProjectionCache;
				_teamScheduling.DayScheduled += OnDayScheduled;
                _teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache, selectedPeriod);
				_teamScheduling.DayScheduled -= OnDayScheduled;

				isTeamScheduling = true;
	        }
			return TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(dailyTeamBlockInfo, day);
	    }

        private bool scheduleSelectedBlockForSameShift(ITeamBlockInfo teamBlockInfo, ISchedulingOptions schedulingOptions, IList<IPerson> selectedPersons, DateOnly day, IShiftProjectionCache suggestedShiftProjectionCache, DateOnlyPeriod selectedPeriod)
        {
            var dailyTeamBlockInfo = new TeamBlockInfo(teamBlockInfo.TeamInfo, new BlockInfo(new DateOnlyPeriod(day, day)));

            if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock( dailyTeamBlockInfo, day)) return true;
	        if (!schedulingOptions.UseGroupScheduling && schedulingOptions.UseTeamBlockPerOption)
			{
				if (_cancelMe)
					return false;
		        foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
		        {
			        if (!selectedPersons.Contains(person)) continue;
			        _teamScheduling.DayScheduled += OnDayScheduled;
			        _teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, suggestedShiftProjectionCache,
			                                               selectedPeriod);
			        _teamScheduling.DayScheduled -= OnDayScheduled;
		        }
			}
			else if (schedulingOptions.UseGroupScheduling && schedulingOptions.UseTeamBlockPerOption)
			{
				if (_cancelMe)
					return false;
				var roleModelShift = suggestedShiftProjectionCache;
				var isTeamScheduling = false;
				foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
				{
					if (!selectedPersons.Contains(person)) continue;
					if (suggestedShiftProjectionCache != null)
					{
						_teamScheduling.DayScheduled += OnDayScheduled;
						
						_teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, suggestedShiftProjectionCache,
															   selectedPeriod);
						_teamScheduling.DayScheduled -= OnDayScheduled;
						suggestedShiftProjectionCache = null;
						isTeamScheduling = true;
						continue;
					}

					var restriction = _restrictionAggregator.AggregatePerDayPerPerson(day, person, teamBlockInfo,
					                                                                  schedulingOptions,
					                                                                  null, isTeamScheduling);
					var shifts = _workShiftFilterService.Filter(day,person, dailyTeamBlockInfo, restriction, roleModelShift, schedulingOptions,
																new WorkShiftFinderResult(dailyTeamBlockInfo.TeamInfo.GroupPerson, day));
					if (shifts == null || shifts.Count <= 0)
						continue;
					var activityInternalData = _skillDayPeriodIntervalDataGenerator.GeneratePerDay(dailyTeamBlockInfo);
					var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																								 schedulingOptions
																									 .WorkShiftLengthHintOption,
																								 schedulingOptions
																									 .UseMinimumPersons,
																								 schedulingOptions
																									 .UseMaximumPersons);
					if (roleModelShift == null)
						roleModelShift = bestShiftProjectionCache;
					_teamScheduling.DayScheduled += OnDayScheduled;
					_teamScheduling.ExecutePerDayPerPerson(person, day, teamBlockInfo, bestShiftProjectionCache, selectedPeriod);
					_teamScheduling.DayScheduled -= OnDayScheduled;
					isTeamScheduling = true;
				}
			}

	        return TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(dailyTeamBlockInfo, day);
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

        
        public void OnDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
			_cancelMe = e.Cancel;
		}
	}
}