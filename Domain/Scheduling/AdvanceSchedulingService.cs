using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface IAdvanceSchedulingService
    {
        bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList);
    }
    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
        private readonly IDynamicBlockFinder _dynamicBlockFinder;
        private readonly ITeamExtractor _teamExtractor;
        private readonly IRestrictionAggregator _restrictionAggregator;
        private readonly IList<IScheduleMatrixPro> _matrixList;
        private readonly IWorkShiftFilterService _workShiftFilterService;
        private readonly ITeamScheduling _teamScheduling;
        private readonly ISchedulingOptions _schedulingOptions;
    	private readonly IWorkShiftSelector _workShiftSelector;
    	private readonly ISkillDayPeriodIntervalData _skillDayPeriodIntervalData;
        private readonly IList<DateOnly> _effectiveDays;
        private readonly IList<DateOnly> _dayOff;
        private IList<DateOnly> _unLockedDays;

        public AdvanceSchedulingService(ISkillDayPeriodIntervalData skillDayPeriodIntervalData,
            IDynamicBlockFinder dynamicBlockFinder, 
            ITeamExtractor teamExtractor,
            IRestrictionAggregator restrictionAggregator,
            IList<IScheduleMatrixPro> matrixList, 
            IWorkShiftFilterService workShiftFilterService,
            ITeamScheduling teamScheduling,
            ISchedulingOptions schedulingOptions,
			IWorkShiftSelector workShiftSelector
            )
        {
            _dynamicBlockFinder = dynamicBlockFinder;
            _teamExtractor = teamExtractor;
            _restrictionAggregator = restrictionAggregator;
            _matrixList = matrixList;
            _workShiftFilterService = workShiftFilterService;
            _teamScheduling = teamScheduling;
            _schedulingOptions = schedulingOptions;
        	_workShiftSelector = workShiftSelector;
        	_skillDayPeriodIntervalData = skillDayPeriodIntervalData;
            _effectiveDays = new List<DateOnly>();
            _dayOff = new List<DateOnly>();
            _unLockedDays = new List<DateOnly>();
        }

        private DateOnly StartDate()
        {
            DateOnly startDate = DateOnly.MinValue ;
            if(_matrixList!= null )
            {
                
                foreach (var scheduleDayPro in _matrixList[0].EffectivePeriodDays.OrderBy( x => x.Day))
                {
                    if (startDate == DateOnly.MinValue && scheduleDayPro.DaySchedulePart().SignificantPart() != SchedulePartView.DayOff)
                        startDate = scheduleDayPro.Day; 
                    if(scheduleDayPro.DaySchedulePart().SignificantPart() == SchedulePartView.DayOff)
                        _dayOff.Add(scheduleDayPro.Day);
                    if (_matrixList[0].UnlockedDays.Contains(scheduleDayPro ))
                        _unLockedDays.Add(scheduleDayPro.Day);
                    _effectiveDays.Add(scheduleDayPro.Day);

                }
            }
            return startDate;
        }

        private DateOnly GetNextDate(DateOnly dateOnly )
        {
            dateOnly = dateOnly.AddDays(1);
            while( _dayOff.Contains(dateOnly ))
               dateOnly = dateOnly.AddDays(1);
            return _effectiveDays.Contains(dateOnly) && _unLockedDays.Contains(dateOnly) ? dateOnly : DateOnly.MinValue;
        }

        public bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList)
        {
            bool success = true;
            var startDate = StartDate();
            
            while (startDate != DateOnly.MinValue )
            {
                //call class that return the teamblock dates for a given date (problem if team members don't have same days off)
                var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays(startDate);

                //call class that finds a random team to schedule
                var groupPerson = _teamExtractor.GetRamdomTeam(startDate);

                //call class that returns the aggregated restrictions for the teamblock (is team member personal skills needed for this?)
                var restriction = _restrictionAggregator.Aggregate(dateOnlyList, groupPerson,_schedulingOptions );

                //call class that returns the aggregated intraday dist based on teamblock dates
                var skillInternalDataList = _skillDayPeriodIntervalData.GetIntervalDistribution(dateOnlyList);

				//temporary
            	ISkill skill =
            		_matrixList.First().Person.PersonPeriodCollection.First().PersonSkillCollection.First().Skill;
				IDictionary<ISkill, IDictionary<TimeSpan, ISkillIntervalData>> askMickeWhyDic = new Dictionary<ISkill, IDictionary<TimeSpan, ISkillIntervalData>>();
				askMickeWhyDic.Add(skill, skillInternalDataList);

                //call class that returns a filtered list of valid workshifts, this class will probably consists of a lot of subclasses 
                // (should we cover for max seats here?)
                var shifts = _workShiftFilterService.Filter(startDate, groupPerson, _matrixList, restriction, _schedulingOptions, null);

                //call class that returns the workshift to use based on valid workshifts, the aggregated intraday dist and other things we need
            	var bestShiftProjectionCache = _workShiftSelector.Select(shifts, askMickeWhyDic,
            	                                                         _schedulingOptions.WorkShiftLengthHintOption,
            	                                                         _schedulingOptions.UseMinimumPersons,
            	                                                         _schedulingOptions.UseMaximumPersons);
                //call class that schedules given date with given workshift on the complete team
                
                //call class that schedules the unscheduled days for the teamblock using the same start time from the given shift, 
                //this class will handle steady state as well as individual
                _teamScheduling.Execute(dateOnlyList, _matrixList, groupPerson, restriction, bestShiftProjectionCache);

               
                //looping on the next block
                startDate = GetNextDate(dateOnlyList.OrderByDescending(x => x.Date).First());
            }

            return success;
        }
    }
}