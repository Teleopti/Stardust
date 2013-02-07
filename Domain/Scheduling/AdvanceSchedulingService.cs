using System.Collections.Generic;
using System.Linq;
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
        private readonly IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
        private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
        private readonly IList<DateOnly> _effectiveDays;
        private readonly IList<DateOnly> _dayOff;
        private IList<DateOnly> _unLockedDays;

        public AdvanceSchedulingService(ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
            IDynamicBlockFinder dynamicBlockFinder, 
            ITeamExtractor teamExtractor,
            IRestrictionAggregator restrictionAggregator,
            IList<IScheduleMatrixPro> matrixList, 
            IWorkShiftFilterService workShiftFilterService,
            ITeamScheduling teamScheduling,
            ISchedulingOptions schedulingOptions,
			IWorkShiftSelector workShiftSelector,
            IGroupPersonBuilderBasedOnContractTime groupPersonBuilderBasedOnContractTime 
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
            _groupPersonBuilderBasedOnContractTime = groupPersonBuilderBasedOnContractTime;
            _skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
            _effectiveDays = new List<DateOnly>();
            _dayOff = new List<DateOnly>();
            _unLockedDays = new List<DateOnly>();
        }

        private DateOnly StartDate()
        {
            DateOnly startDate = DateOnly.MinValue ;
            if(_matrixList!= null )
            {
            	var openMatrixList = _matrixList.Where(x => x.Person.Equals(_matrixList[0].Person));
            	foreach (var scheduleMatrixPro in openMatrixList)
            	{
					foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays.OrderBy(x => x.Day))
					{
						if (startDate == DateOnly.MinValue && scheduleDayPro.DaySchedulePart().SignificantPart() != SchedulePartView.DayOff)
							startDate = scheduleDayPro.Day;
						if (scheduleDayPro.DaySchedulePart().SignificantPart() == SchedulePartView.DayOff)
							_dayOff.Add(scheduleDayPro.Day);
						if (scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
							_unLockedDays.Add(scheduleDayPro.Day);
						_effectiveDays.Add(scheduleDayPro.Day);
					}
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
            var startDate = StartDate();
            
            while (startDate != DateOnly.MinValue )
            {
                //call class that return the teamblock dates for a given date (problem if team members don't have same days off)
                var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays( startDate );
                
                var fullGroupPerson = _teamExtractor.GetRandomTeam(startDate);
                var groupPersonList = _groupPersonBuilderBasedOnContractTime.SplitTeams(fullGroupPerson, startDate);
                foreach(var groupPerson in groupPersonList )
                {
                	var groupMatrixList = GetScheduleMatrixProList(groupPerson, startDate);
                    //call class that returns the aggregated restrictions for the teamblock (is team member personal skills needed for this?)
                    var restriction = GetEffectiveRestriction(groupPerson, dateOnlyList);

                    //call class that returns the aggregated intraday dist based on teamblock dates ???? consider the priority and understaffing
                    var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(fullGroupPerson, dateOnlyList);

                    //call class that returns a filtered list of valid workshifts, this class will probably consists of a lot of subclasses 
                    // (should we cover for max seats here?) ????
					var shifts = GetShiftProjectionCaches(restriction, groupMatrixList, groupPerson, startDate);

                    if(shifts!=null && shifts.Count>0)
                    {
                        //call class that returns the workshift to use based on valid workshifts, the aggregated intraday dist and other things we need ???
                        var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
                                                                                 _schedulingOptions.WorkShiftLengthHintOption,
                                                                                 _schedulingOptions.UseMinimumPersons,
                                                                                 _schedulingOptions.UseMaximumPersons);
                        
                        //call class that schedules given date with given workshift on the complete team
                        //call class that schedules the unscheduled days for the teamblock using the same start time from the given shift, 
                        //this class will handle steady state as well as individual
                        _teamScheduling.Execute(dateOnlyList, groupMatrixList, groupPerson, restriction, bestShiftProjectionCache, _unLockedDays);
                    }
                    
                }
                startDate = GetNextDate(dateOnlyList.OrderByDescending(x => x.Date).First());
            }

            return true;
        }

        private IList<IShiftProjectionCache> GetShiftProjectionCaches(IEffectiveRestriction restriction, List<IScheduleMatrixPro> groupMatrixList, IGroupPerson groupPerson,
                                               DateOnly startDate)
        {
            var shifts = _workShiftFilterService.Filter(startDate, groupPerson, groupMatrixList, restriction, _schedulingOptions);
            return shifts;
        }

        private List<IScheduleMatrixPro> GetScheduleMatrixProList(IGroupPerson groupPerson, DateOnly startDate)
        {
            var person = groupPerson;
            var date = startDate;
            var groupMatrixList =
                _matrixList.Where(x => person.GroupMembers.Contains(x.Person) && x.SchedulePeriod.DateOnlyPeriod.Contains(date))
                    .ToList();
            return groupMatrixList;
        }

        private IEffectiveRestriction GetEffectiveRestriction(IGroupPerson groupPerson, IList<DateOnly> dateOnlyList)
        {
            var restriction = _restrictionAggregator.Aggregate(dateOnlyList, groupPerson, _schedulingOptions);
            return restriction;
        }
    }
}