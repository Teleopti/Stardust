using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning.Scheduling
{
    public class WorkShiftMinMaxCalculator : IWorkShiftMinMaxCalculator
    {
        private readonly IPossibleMinMaxWorkShiftLengthExtractor _possibleMinMaxWorkShiftLengthExtractor;
        private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
        private readonly IWorkShiftWeekMinMaxCalculator _weekCalculator;
        private IDictionary<DateOnly, MinMax<TimeSpan>> _possibleMinMaxWorkShiftLengthState;

        public WorkShiftMinMaxCalculator(IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor, 
            ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, 
            IWorkShiftWeekMinMaxCalculator weekCalculator)
        {
            _possibleMinMaxWorkShiftLengthExtractor = possibleMinMaxWorkShiftLengthExtractor;
            _schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
            _weekCalculator = weekCalculator;
        }

        public bool IsPeriodInLegalState(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            return (PeriodLegalStateStatus(matrix, schedulingOptions) == 0);
        }

        public int PeriodLegalStateStatus(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            MinMax<TimeSpan> minMax = currentMinMax(null, matrix, schedulingOptions);
            TimeSpan corr = calculateSumCorrection(null, matrix, WeekCount(matrix), schedulingOptions);


            if (minMax.Minimum > _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix).EndTime)
                return 1; // over
            if (minMax.Maximum.Subtract(corr) < _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix).StartTime)
                return -1; // under

            return 0;
        }

        public bool IsWeekInLegalState(int weekIndex, IScheduleMatrixPro matrix, ISchedulingOptions options)
        {
            IDictionary<DateOnly, MinMax<TimeSpan>> dic = PossibleMinMaxWorkShiftLengths(matrix, options); 
            bool ret = _weekCalculator.IsInLegalState(weekIndex, dic, matrix);
            return ret;
        }

        public bool IsWeekInLegalState(DateOnly dateInWeek, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            DateOnly startDate = matrix.FullWeeksPeriodDays[0].Day;
            int index = 0;
            while (startDate <= matrix.FullWeeksPeriodDays[matrix.FullWeeksPeriodDays.Count - 1].Day)
            {
                if (dateInWeek >= startDate && dateInWeek <= startDate.AddDays(6))
                    return IsWeekInLegalState(index, matrix, schedulingOptions);
                index++;
                startDate = startDate.AddDays(7);
            }

            throw new ArgumentOutOfRangeException("dateInWeek", dateInWeek, "Value is outside matrix fullweek period");
        }

        private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
        {
            return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
        }

        private static int weekIndexFromDate(DateOnly dateInWeek, IScheduleMatrixPro matrix)
        {
            DateOnly startDate = matrix.FullWeeksPeriodDays[0].Day;
            int index = 0;
            while (startDate <= matrix.FullWeeksPeriodDays[matrix.FullWeeksPeriodDays.Count - 1].Day)
            {
                if (dateInWeek >= startDate && dateInWeek <= startDate.AddDays(6))
                    return index;
                index++;
                startDate = startDate.AddDays(7);
            }

            throw new ArgumentOutOfRangeException("dateInWeek", dateInWeek, "Value is outside matrix fullweek period");
        }

        public MinMax<TimeSpan>? MinMaxAllowedShiftContractTime(DateOnly dayToSchedule, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            MinMax<TimeSpan> minMax = currentMinMax(dayToSchedule, matrix, schedulingOptions);
            int numberOfWeeks = WeekCount(matrix);
            TimeSpan sumCorrection = calculateSumCorrection(dayToSchedule, matrix, numberOfWeeks, schedulingOptions);

            TimeSpan corrMax = minMax.Maximum.Subtract(sumCorrection);
            TimeSpan minLenght = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix).StartTime.Subtract(corrMax);



            TimeSpan minForDay = PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions)[dayToSchedule].Minimum;
            if (minForDay > minLenght)
                minLenght = minForDay;

            TimeSpan maxLength = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix).EndTime.Subtract(minMax.Minimum);
            TimeSpan maxForDay = PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions)[dayToSchedule].Maximum;
            if (maxForDay < maxLength)
                maxLength = maxForDay;

            int weekIndex = weekIndexFromDate(dayToSchedule, matrix);
            bool skipThisWeek = false;
            if (weekIndex == 0 || weekIndex == numberOfWeeks - 1)
                skipThisWeek = skipWeekCheck(matrix, firstDateInWeekIndex(weekIndex, matrix));

            if (!skipThisWeek)
            {
                TimeSpan? maxLengthByWeek = _weekCalculator.MaxAllowedLength(weekIndex, PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions), dayToSchedule, matrix);

                if (maxLengthByWeek.HasValue)
                {
                    if (maxLengthByWeek.Value < maxLength)
                        maxLength = maxLengthByWeek.Value;
                }
                else
                {
                    return null;
                }
            }


            if (maxLength < minLenght)
                return null;

            return new MinMax<TimeSpan>(minLenght, maxLength);
        }

        private TimeSpan calculateSumCorrection(DateOnly? dayToSchedule, IScheduleMatrixPro matrix, int numberOfWeeks, ISchedulingOptions schedulingOptions)
        {
            TimeSpan sumCorrection = TimeSpan.Zero;

            
            if(numberOfWeeks > 1)
            {
                for (int i = 0; i < numberOfWeeks; i++)
                {
                    bool skipWeek = false;
                    if (i == 0 || i == numberOfWeeks - 1)
                        skipWeek = skipWeekCheck(matrix, firstDateInWeekIndex(i, matrix));

                    if(!skipWeek)
                        sumCorrection = sumCorrection.Add(_weekCalculator.CorrectionDiff(i, PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions), dayToSchedule, matrix));
                }
            }
            return sumCorrection;
        }

        public MinMax<TimeSpan> PossibleMinMaxTimeForPeriod(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
    	{
    		return currentMinMax(null,matrix, schedulingOptions);
    	}

        public int WeekCount(IScheduleMatrixPro matrix)
        {
            return matrix.FullWeeksPeriodDays.Count / 7; 
        }

        public void ResetCache()
        {
            _possibleMinMaxWorkShiftLengthState = null;
            _possibleMinMaxWorkShiftLengthExtractor.ResetCache();
        }

        private static bool skipWeekCheck(IScheduleMatrixPro matrix, DateOnly dateToCheck)
        {
            var contract = matrix.SchedulePeriod.Contract;
            var weekPeriod = DateHelper.GetWeekPeriod(dateToCheck, matrix.Person.FirstDayOfWeek);
            IPersonPeriod period = matrix.Person.Period(matrix.SchedulePeriod.DateOnlyPeriod.StartDate);

            if (weekPeriod.Contains(matrix.SchedulePeriod.DateOnlyPeriod.StartDate.AddDays(-1)))
            {
                IPersonPeriod previousPeriod = matrix.Person.PreviousPeriod(period);
                if (previousPeriod != null)
                {
                    if (contract.WorkTimeDirective.MaxTimePerWeek != previousPeriod.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek)
                        return true;
                }

                IVirtualSchedulePeriod schedulePeriod =
                    matrix.Person.VirtualSchedulePeriod(matrix.SchedulePeriod.DateOnlyPeriod.StartDate.AddDays(-1));
                if (!schedulePeriod.IsValid)
                    return true;
            }
            if (weekPeriod.Contains(matrix.SchedulePeriod.DateOnlyPeriod.EndDate.AddDays(1)))
            {
                IPersonPeriod nextPeriod = matrix.Person.NextPeriod(period);
                if (nextPeriod != null)
                {
                    if (contract.WorkTimeDirective.MaxTimePerWeek != nextPeriod.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek)
                        return true;
                }
            }

            return false;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IDictionary<DateOnly, MinMax<TimeSpan>> PossibleMinMaxWorkShiftLengths(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {

            if (_possibleMinMaxWorkShiftLengthState == null)
            {
                _possibleMinMaxWorkShiftLengthState = new Dictionary<DateOnly, MinMax<TimeSpan>>();
                foreach (var scheduleDayPro in matrix.FullWeeksPeriodDays)
                {
                    _possibleMinMaxWorkShiftLengthState.Add(scheduleDayPro.Day,
                        _possibleMinMaxWorkShiftLengthExtractor.PossibleLengthsForDate(scheduleDayPro.Day, matrix, schedulingOptions));
                }
            }

            return _possibleMinMaxWorkShiftLengthState;
        }

        private MinMax<TimeSpan> currentMinMax(DateOnly? dayToSchedule, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            TimeSpan min = TimeSpan.Zero;
            TimeSpan max = TimeSpan.Zero;
            foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
            {
                TimeSpan contractTime;
                if (dayToSchedule.HasValue)
                {
                    if (scheduleDayPro.Day == dayToSchedule.Value)
                    {
                        contractTime = TimeSpan.Zero;
                        min = min.Add(contractTime);
                        max = max.Add(contractTime);
                        continue;
                    }
                }

	            var part = scheduleDayPro.DaySchedulePart();
                SchedulePartView significant = part.SignificantPart();
                if (significant == SchedulePartView.MainShift || significant == SchedulePartView.FullDayAbsence)
                {
                    contractTime = part.ProjectionService().CreateProjection().ContractTime();
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
                        min = min.Add(PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions)[scheduleDayPro.Day].Minimum);
                        max = max.Add(PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions)[scheduleDayPro.Day].Maximum);
                    }
                }
            }

            return new MinMax<TimeSpan>(min, max);
        }
    }
}
