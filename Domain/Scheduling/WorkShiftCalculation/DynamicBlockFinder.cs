using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface IDynamicBlockFinder
    {
        List<DateOnly> ExtractBlockDays(DateOnly startDateOnly);
    }

    public class DynamicBlockFinder : IDynamicBlockFinder
    {
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        private readonly ISchedulingOptions _schedulingOptions;

        public DynamicBlockFinder(ISchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            SchedulingResultStateHolder = schedulingResultStateHolder;
            _schedulingOptions = schedulingOptions;
        }

        public List<DateOnly> ExtractBlockDays(DateOnly startDateOnly)
        {
            var selectedPeriod = SchedulingResultStateHolder.Schedules.Period.LoadedPeriod();
            var timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
            var retList = new List<DateOnly>();
            if(_schedulingOptions.UsePeriodAsBlock )
            {
                retList = selectedPeriod.ToDateOnlyPeriod(timeZone).DayCollection().ToList() ;
            }
            else if (_schedulingOptions.UseCalenderWeekAsBlock)
            {
                var scheduleList = selectedPeriod.ToDateOnlyPeriod(timeZone).DayCollection();
                var dateOnlyList = new List<DateOnly >();
                foreach (var dateOnly in selectedPeriod.ToDateOnlyPeriod(timeZone).DayCollection().Where(day => day >= startDateOnly))
                {
                    if (dateOnly.DayOfWeek.Equals(DayOfWeek.Sunday) || dateOnly.DayOfWeek.Equals(DayOfWeek.Saturday ))
                    {
                        break;
                    }
                    if (scheduleList.Contains(dateOnly))
                    {
                        dateOnlyList.Add(dateOnly);
                    }
                }

                retList = dateOnlyList.ToList();
            }
            else if(_schedulingOptions.UseTwoDaysOffAsBlock )
            {
                var dateOnlyList =
                    selectedPeriod.ToDateOnlyPeriod(timeZone).DayCollection().Where(day => day >= startDateOnly)
                        .ToList();
                var dayOffList = GetDaysOffFromSchedule(dateOnlyList );
                var extractedBlock = new List<DateOnly>();
                for (int i=0;i<dateOnlyList.Count ;i++)
                {
                    if(i+1 < dateOnlyList.Count && dayOffList.Contains(dateOnlyList[i]) &&  dayOffList.Contains(dateOnlyList[i]))
                    {
                        break;
                    }
                    extractedBlock.Add(dateOnlyList[i]);
                }
                retList = extractedBlock.ToList() ;
            }
           
           
            return retList;
        }

        private List<DateOnly > GetDaysOffFromSchedule(IEnumerable<DateOnly> dateOnlyListForFullPeriod )
        {
            return (from dateOnly in dateOnlyListForFullPeriod let scheduleDayList = SchedulingResultStateHolder.Schedules.SchedulesForDay(dateOnly) 
                    where scheduleDayList.Any(schedule => schedule.SignificantPart() == SchedulePartView.DayOff) select dateOnly).ToList();
        }
    }
}