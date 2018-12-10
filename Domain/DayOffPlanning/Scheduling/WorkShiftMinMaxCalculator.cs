using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.DayOffPlanning.Scheduling
{
    public class WorkShiftMinMaxCalculator : IWorkShiftMinMaxCalculator
    {
        private readonly IPossibleMinMaxWorkShiftLengthExtractor _possibleMinMaxWorkShiftLengthExtractor;
        private readonly ISchedulePeriodTargetTimeCalculator _schedulePeriodTargetTimeCalculator;
        private readonly IWorkShiftWeekMinMaxCalculator _weekCalculator;
        private IDictionary<DateOnly, MinMax<TimeSpan>> _possibleMinMaxWorkShiftLengthState;
	    private readonly WorkShiftMinMaxCalculatorSkipWeekCheck _workShiftMinMaxCalculatorSkipWeekCheck = new WorkShiftMinMaxCalculatorSkipWeekCheck();

	    public WorkShiftMinMaxCalculator(IPossibleMinMaxWorkShiftLengthExtractor possibleMinMaxWorkShiftLengthExtractor, 
            ISchedulePeriodTargetTimeCalculator schedulePeriodTargetTimeCalculator, 
            IWorkShiftWeekMinMaxCalculator weekCalculator)
        {
            _possibleMinMaxWorkShiftLengthExtractor = possibleMinMaxWorkShiftLengthExtractor;
            _schedulePeriodTargetTimeCalculator = schedulePeriodTargetTimeCalculator;
            _weekCalculator = weekCalculator;
        }

        public bool IsPeriodInLegalState(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
        {
            return PeriodLegalStateStatus(matrix, schedulingOptions) == 0;
        }

        public int PeriodLegalStateStatus(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
        {
            MinMax<TimeSpan> minMax = currentMinMax(null, matrix, schedulingOptions);
            TimeSpan corr = calculateSumCorrection(null, matrix, WeekCount(matrix), schedulingOptions);


	        var targetWithTolerance = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix);
	        if (minMax.Minimum > targetWithTolerance.EndTime)
                return 1; // over
            if (minMax.Maximum.Subtract(corr) < targetWithTolerance.StartTime)
                return -1; // under

            return 0;
        }

        public bool IsWeekInLegalState(int weekIndex, IScheduleMatrixPro matrix, SchedulingOptions options)
        {
            IDictionary<DateOnly, MinMax<TimeSpan>> dic = PossibleMinMaxWorkShiftLengths(matrix, options, null); 
            bool ret = _weekCalculator.IsInLegalState(weekIndex, dic, matrix);
            return ret;
        }

        public bool IsWeekInLegalState(DateOnly dateInWeek, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
        {
            DateOnly startDate = matrix.FullWeeksPeriodDays[0].Day;
            int index = 0;
            while (startDate <= matrix.FullWeeksPeriodDays[matrix.FullWeeksPeriodDays.Length - 1].Day)
            {
                if (dateInWeek >= startDate && dateInWeek <= startDate.AddDays(6))
                    return IsWeekInLegalState(index, matrix, schedulingOptions);
                index++;
                startDate = startDate.AddDays(7);
            }

            throw new ArgumentOutOfRangeException(nameof(dateInWeek), dateInWeek, "Value is outside matrix fullweek period");
        }

        private static DateOnly firstDateInWeekIndex(int weekIndex, IScheduleMatrixPro matrix)
        {
            return matrix.FullWeeksPeriodDays[weekIndex * 7].Day;
        }

        private static int weekIndexFromDate(DateOnly dateInWeek, IScheduleMatrixPro matrix)
        {
            DateOnly startDate = matrix.FullWeeksPeriodDays[0].Day;
            int index = 0;
            while (startDate <= matrix.FullWeeksPeriodDays[matrix.FullWeeksPeriodDays.Length - 1].Day)
            {
                if (dateInWeek >= startDate && dateInWeek <= startDate.AddDays(6))
                    return index;
                index++;
                startDate = startDate.AddDays(7);
            }

            throw new ArgumentOutOfRangeException(nameof(dateInWeek), dateInWeek, "Value is outside matrix fullweek period");
        }

        public MinMax<TimeSpan>? MinMaxAllowedShiftContractTime(DateOnly dayToSchedule, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
        {
            MinMax<TimeSpan> minMax = currentMinMax(dayToSchedule, matrix, schedulingOptions);
            int numberOfWeeks = WeekCount(matrix);
            TimeSpan sumCorrection = calculateSumCorrection(dayToSchedule, matrix, numberOfWeeks, schedulingOptions);

            TimeSpan corrMax = minMax.Maximum.Subtract(sumCorrection);
	        var targetWithTolerance = _schedulePeriodTargetTimeCalculator.TargetWithTolerance(matrix);
	        TimeSpan minLength = targetWithTolerance.StartTime.Subtract(corrMax);

	        var possibleMinMaxWorkShiftLengths = PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions, null);
	        var possibleMinMaxWorkShiftLengthForDay = possibleMinMaxWorkShiftLengths[dayToSchedule];
	        TimeSpan minForDay = possibleMinMaxWorkShiftLengthForDay.Minimum;
            if (minForDay > minLength)
                minLength = minForDay;

            TimeSpan maxLength = targetWithTolerance.EndTime.Subtract(minMax.Minimum);
            TimeSpan maxForDay = possibleMinMaxWorkShiftLengthForDay.Maximum;
            if (maxForDay < maxLength)
                maxLength = maxForDay;

            int weekIndex = weekIndexFromDate(dayToSchedule, matrix);
            bool skipThisWeek = false;
	        if (weekIndex == 0 || weekIndex == numberOfWeeks - 1)
				skipThisWeek = _workShiftMinMaxCalculatorSkipWeekCheck.SkipWeekCheck(matrix, firstDateInWeekIndex(weekIndex, matrix));
	        
            if (!skipThisWeek)
            {
                TimeSpan? maxLengthByWeek = _weekCalculator.MaxAllowedLength(weekIndex, possibleMinMaxWorkShiftLengths, dayToSchedule, matrix);

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


            if (maxLength < minLength)
                return null;

            return new MinMax<TimeSpan>(minLength, maxLength);
        }

        private TimeSpan calculateSumCorrection(DateOnly? dayToSchedule, IScheduleMatrixPro matrix, int numberOfWeeks, SchedulingOptions schedulingOptions)
        {
            TimeSpan sumCorrection = TimeSpan.Zero;
            if(numberOfWeeks > 1)
            {
                for (int i = 0; i < numberOfWeeks; i++)
                {
                    bool skipWeek = false;
                    if (i == 0 || i == numberOfWeeks - 1)
                        skipWeek = _workShiftMinMaxCalculatorSkipWeekCheck.SkipWeekCheck(matrix, firstDateInWeekIndex(i, matrix));

                    if(!skipWeek)
                        sumCorrection = sumCorrection.Add(_weekCalculator.CorrectionDiff(i, PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions, null), dayToSchedule, matrix));
                }
            }
            return sumCorrection;
        }

        public MinMax<TimeSpan> PossibleMinMaxTimeForPeriod(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
    	{
    		return currentMinMax(null,matrix, schedulingOptions);
    	}

        public int WeekCount(IScheduleMatrixPro matrix)
        {
            return matrix.FullWeeksPeriodDays.Length / 7; 
        }

        public void ResetCache()
        {
            _possibleMinMaxWorkShiftLengthState = null;
            _possibleMinMaxWorkShiftLengthExtractor.ResetCache();
        }

        public IDictionary<DateOnly, MinMax<TimeSpan>> PossibleMinMaxWorkShiftLengths(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, OpenHoursSkillResult openHoursSkillResult)
        {
	        return _possibleMinMaxWorkShiftLengthState ??
				   (_possibleMinMaxWorkShiftLengthState =
					   matrix.FullWeeksPeriodDays.ToDictionary(k => k.Day,
						   v => _possibleMinMaxWorkShiftLengthExtractor.PossibleLengthsForDate(v.Day, matrix, schedulingOptions, openHoursSkillResult)));
        }

        private MinMax<TimeSpan> currentMinMax(DateOnly? dayToSchedule, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
        {
			var min = TimeSpan.Zero;
			var max = TimeSpan.Zero;
			var possibleMinMaxWorkShiftLength = new Lazy<IDictionary<DateOnly, MinMax<TimeSpan>>>(() => PossibleMinMaxWorkShiftLengths(matrix, schedulingOptions, null));
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
				var significant = part.SignificantPart();
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
						var possibleMinMaxWorkShiftLengthForDate = possibleMinMaxWorkShiftLength.Value[scheduleDayPro.Day];
						min = min.Add(possibleMinMaxWorkShiftLengthForDate.Minimum);
						max = max.Add(possibleMinMaxWorkShiftLengthForDate.Maximum);
					}
				}
			}

			return new MinMax<TimeSpan>(min, max);
		}
    }
}
