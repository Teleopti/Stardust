using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface IAdvanceSchedulingService
    {
        bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList, IList<IScheduleMatrixPro> allPersonMatrixList, IList<IScheduleMatrixPro> selectedPersonMatrixList);
    }
    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private readonly IDynamicBlockFinder _dynamicBlockFinder;
        private readonly IRestrictionAggregator _restrictionAggregator;
        private readonly IWorkShiftFilterService _workShiftFilterService;
        private readonly ITeamScheduling _teamScheduling;
        private readonly ISchedulingOptions _schedulingOptions;
    	private readonly IWorkShiftSelector _workShiftSelector;
        private readonly IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
        private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;


        public AdvanceSchedulingService(ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
            IDynamicBlockFinder dynamicBlockFinder, 
            IRestrictionAggregator restrictionAggregator,
            IWorkShiftFilterService workShiftFilterService,
            ITeamScheduling teamScheduling,
            ISchedulingOptions schedulingOptions,
			IWorkShiftSelector workShiftSelector,
            IGroupPersonBuilderBasedOnContractTime groupPersonBuilderBasedOnContractTime ,
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization 
            )
        {
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
            _dynamicBlockFinder = dynamicBlockFinder;
            _restrictionAggregator = restrictionAggregator;
            _workShiftFilterService = workShiftFilterService;
            _teamScheduling = teamScheduling;
            _schedulingOptions = schedulingOptions;
        	_workShiftSelector = workShiftSelector;
            _groupPersonBuilderBasedOnContractTime = groupPersonBuilderBasedOnContractTime;
            _skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
        }

        //private DateOnly StartDate()
        //{
        //    var startDate = DateOnly.MinValue;
        //    if(_matrixList!= null )
        //    {
        //        for (var i = 0; i < _matrixList.Count; i++)
        //        {
        //            var openMatrixList = _matrixList.Where(x => x.Person.Equals(_matrixList[i].Person));
        //            foreach (var scheduleMatrixPro in openMatrixList)
        //            {
        //                foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays.OrderBy(x => x.Day))
        //                {
        //                    var daySignificantPart = scheduleDayPro.DaySchedulePart().SignificantPart();
        //                    if (startDate == DateOnly.MinValue &&
        //                        (daySignificantPart != SchedulePartView.DayOff &&
        //                         daySignificantPart != SchedulePartView.ContractDayOff &&
        //                         daySignificantPart != SchedulePartView.FullDayAbsence))
        //                    {
        //                        startDate = scheduleDayPro.Day;
        //                    }
        //                    if (daySignificantPart == SchedulePartView.DayOff)
        //                        _dayOff.Add(scheduleDayPro.Day);
        //                    _effectiveDays.Add(scheduleDayPro.Day);
        //                    if (scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
        //                        _unLockedDays.Add(scheduleDayPro.Day);
        //                }
        //            }
        //        }
                

        //    }
        //    return startDate;
        //}

        private DateOnly StartDate(IList<IScheduleMatrixPro> matrixList, out  List<DateOnly> dayOff, out  List<DateOnly> effectiveDays, out  List<DateOnly> unLockedDays)
        {
            var startDate = DateOnly.MinValue;
            dayOff = new List<DateOnly>();
            effectiveDays = new List<DateOnly>();
            unLockedDays = new List<DateOnly>();
            
            if (matrixList != null)
            {
                for (var i = 0; i < matrixList.Count; i++)
                {
                    var openMatrixList = matrixList.Where(x => x.Person.Equals(matrixList[i].Person));
                    foreach (var scheduleMatrixPro in openMatrixList)
                    {
                        foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays.OrderBy(x => x.Day))
                        {
                            var daySignificantPart = scheduleDayPro.DaySchedulePart().SignificantPart();
                            
                            if (startDate == DateOnly.MinValue &&
                                (daySignificantPart != SchedulePartView.DayOff &&
                                 daySignificantPart != SchedulePartView.ContractDayOff &&
                                 daySignificantPart != SchedulePartView.FullDayAbsence))
                            {
                                startDate = scheduleDayPro.Day;
                            }
                            
                            if (daySignificantPart == SchedulePartView.DayOff)
                                dayOff.Add(scheduleDayPro.Day);
                            effectiveDays.Add(scheduleDayPro.Day);
                            
                            if (scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
                                unLockedDays.Add(scheduleDayPro.Day);
                        }
                    }
                }


            }
            return startDate;
        }

        private DateOnly GetNextDate(DateOnly dateOnly, List<DateOnly> effectiveDays, List<DateOnly> daysOff, List<DateOnly> unLockedDays)
        {
            dateOnly = dateOnly.AddDays(1);
            while (daysOff.Contains(dateOnly))
               dateOnly = dateOnly.AddDays(1);
            return effectiveDays.Contains(dateOnly) && unLockedDays.Contains(dateOnly) ? dateOnly : DateOnly.MinValue;
        }

        public bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList, IList<IScheduleMatrixPro> allPersonMatrixList, IList<IScheduleMatrixPro> selectedPersonMatrixList)
        {
            List<DateOnly> dayOff,effectiveDays,unLockedDays;

            var startDate = StartDate(selectedPersonMatrixList, out dayOff, out effectiveDays, out unLockedDays);

            var selectedPerson = selectedPersonMatrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();
            
            while (startDate != DateOnly.MinValue )
            {
                //call class that return the teamblock dates for a given date (problem if team members don't have same days off)
                var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays( startDate );
                var allGroupPersonListOnStartDate = new List<IGroupPerson>();
                
                foreach (var person in selectedPerson)
                {
                    allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(person,startDate ));
                }
                
                foreach (var fullGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
                {
                    var groupPersonList = _groupPersonBuilderBasedOnContractTime.SplitTeams(fullGroupPerson, startDate);
                    foreach (var groupPerson in groupPersonList)
                    {
                        var groupMatrixList = GetScheduleMatrixProList(groupPerson, startDate, allPersonMatrixList);
                        //call class that returns the aggregated restrictions for the teamblock (is team member personal skills needed for this?)
                        var restriction = GetEffectiveRestriction(groupPerson, dateOnlyList);

                        //call class that returns the aggregated intraday dist based on teamblock dates ???? consider the priority and understaffing
                        var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(fullGroupPerson, dateOnlyList);

                        //call class that returns a filtered list of valid workshifts, this class will probably consists of a lot of subclasses 
                        // (should we cover for max seats here?) ????
						var shifts = GetShiftProjectionCaches(restriction, groupMatrixList, groupPerson, startDate);

                        if (shifts != null && shifts.Count > 0)
                        {
                            //call class that returns the workshift to use based on valid workshifts, the aggregated intraday dist and other things we need ???
                            var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
                                                                                        _schedulingOptions.WorkShiftLengthHintOption,
                                                                                        _schedulingOptions.UseMinimumPersons,
                                                                                        _schedulingOptions.UseMaximumPersons);

                            //call class that schedules given date with given workshift on the complete team
                            //call class that schedules the unscheduled days for the teamblock using the same start time from the given shift, 
                            //this class will handle steady state as well as individual
                            _teamScheduling.Execute(startDate, dateOnlyList, groupMatrixList, groupPerson, restriction, bestShiftProjectionCache, unLockedDays,selectedPerson );
                        }

                    }
                }
                startDate = GetNextDate(dateOnlyList.OrderByDescending(x => x.Date).First(), effectiveDays,dayOff,unLockedDays );
                
            }

            return true;
        }

        private IList<IShiftProjectionCache> GetShiftProjectionCaches(IEffectiveRestriction restriction, IList<IScheduleMatrixPro> groupMatrixList, IGroupPerson groupPerson,
                                               DateOnly startDate)
        {
            var shifts = _workShiftFilterService.Filter(startDate, groupPerson, groupMatrixList, restriction, _schedulingOptions);
            return shifts;
        }

        private List<IScheduleMatrixPro> GetScheduleMatrixProList(IGroupPerson groupPerson, DateOnly startDate, IEnumerable<IScheduleMatrixPro> matrixList)
        {
            var person = groupPerson;
            var date = startDate;
            var groupMatrixList =
                matrixList.Where(x => person.GroupMembers.Contains(x.Person) && x.SchedulePeriod.DateOnlyPeriod.Contains(date))
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