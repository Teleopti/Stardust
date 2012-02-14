using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning.Scheduling
{
    public class WorkShiftWeekMinMaxCalculator : IWorkShiftWeekMinMaxCalculator
    {
        
        public bool IsInLegalState(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, IScheduleMatrixPro matrix)
        {
            var contract = matrix.SchedulePeriod.Contract;
            TimeSpan maxWeekWorktime =
                contract.WorkTimeDirective.MaxTimePerWeek;

            TimeSpan min = currentMinMax(weekIndex, possibleMinMaxWorkShiftLengths, null, matrix).Minimum;
            if (min > maxWeekWorktime)
                return false;

            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public TimeSpan CorrectionDiff(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, DateOnly? dayToSchedule, IScheduleMatrixPro matrix)
        {
            var contract = matrix.SchedulePeriod.Contract;
            TimeSpan maxWeekWorktime =
                contract.WorkTimeDirective.MaxTimePerWeek;
            MinMax<TimeSpan> minMax = currentMinMax(weekIndex, possibleMinMaxWorkShiftLengths, dayToSchedule, matrix);
            TimeSpan max = minMax.Maximum;

            TimeSpan corr = max;
            if (max > maxWeekWorktime)
                corr = maxWeekWorktime;
            TimeSpan corrDiff = max - corr;

            return corrDiff;
        }

        public TimeSpan? MaxAllowedLength(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, DateOnly dayToSchedule, IScheduleMatrixPro matrix)
        {
            var contract = matrix.SchedulePeriod.Contract;
            TimeSpan maxWeekWorktime =
                contract.WorkTimeDirective.MaxTimePerWeek;
            MinMax<TimeSpan> minMax = currentMinMax(weekIndex, possibleMinMaxWorkShiftLengths, dayToSchedule, matrix);

            TimeSpan max = maxWeekWorktime.Subtract(minMax.Minimum);
            if (max > possibleMinMaxWorkShiftLengths[dayToSchedule].Maximum)
                return possibleMinMaxWorkShiftLengths[dayToSchedule].Maximum;

            if(max < possibleMinMaxWorkShiftLengths[dayToSchedule].Minimum)
                return null;

            return max;
        }

        private static MinMax<TimeSpan> currentMinMax(int weekIndex, IDictionary<DateOnly, MinMax<TimeSpan>> possibleMinMaxWorkShiftLengths, DateOnly? dayToSchedule, IScheduleMatrixPro matrix)
        {
            TimeSpan min = TimeSpan.Zero;
            TimeSpan max = TimeSpan.Zero;
            DateOnly firstDate = possibleMinMaxWorkShiftLengths.Keys.FirstOrDefault();
            for (int i = 0; i <= 6; i++)
            {
                int dayIndex;
                checked
                {
                    dayIndex = (weekIndex * 7) + i;
                }
                IScheduleDayPro scheduleDayPro = matrix.GetScheduleDayByKey(firstDate.AddDays(dayIndex));

                TimeSpan contractTime;
                if(dayToSchedule.HasValue)
                {
                    if(scheduleDayPro.Day == dayToSchedule.Value)
                    {
                        contractTime = TimeSpan.Zero;
                        min = min.Add(contractTime);
                        max = max.Add(contractTime);
                        continue;
                    }
                }


            	SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
				if (significant == SchedulePartView.MainShift || significant == SchedulePartView.FullDayAbsence)
                {
                    contractTime =
                        scheduleDayPro.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();
                    min = min.Add(contractTime);
                    max = max.Add(contractTime);
                }
                else
                {
					if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
                    {
                        contractTime = TimeSpan.Zero;
                        min = min.Add(contractTime);
                        max = max.Add(contractTime);
                    }
                    else
                    {
                        min = min.Add(possibleMinMaxWorkShiftLengths[scheduleDayPro.Day].Minimum);
                        if (matrix.EffectivePeriodDays.Contains(scheduleDayPro))
                            max = max.Add(possibleMinMaxWorkShiftLengths[scheduleDayPro.Day].Maximum);
                        else //day is outside schedule period so min should be added
                            max = max.Add(possibleMinMaxWorkShiftLengths[scheduleDayPro.Day].Minimum);
                    }
                }
            }
            return new MinMax<TimeSpan>(min, max);
        }
    }
}
