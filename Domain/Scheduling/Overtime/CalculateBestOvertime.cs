﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface ICalculateBestOvertime
    {
        IList<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDurantion, MinMax<TimeSpan> overtimeSpecifiedPeriod,  IList<OvertimePeriodValue> overtimePeriodValueMappedDat, IScheduleDay scheduleDay, int minimumResolution, bool onlyOvertimeAvaialbility);
    }

    public class CalculateBestOvertime : ICalculateBestOvertime
    {
	    private readonly IAnalyzePersonAccordingToAvailability _analyzePersonAccordingToAvailability;

	    public CalculateBestOvertime(IAnalyzePersonAccordingToAvailability analyzePersonAccordingToAvailability)
	    {
		    _analyzePersonAccordingToAvailability = analyzePersonAccordingToAvailability;
	    }

		public IList<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDurantion, MinMax<TimeSpan> overtimeSpecifiedPeriod, IList<OvertimePeriodValue> overtimePeriodValueMappedData, IScheduleDay scheduleDay, int minimumResolution, bool onlyOvertimeAvaialbility)
        {
            var possibleOvertimeDurationsToCalculate = new List<TimeSpan>();
            for (int minutes = minimumResolution; minutes <= overtimeDurantion.Maximum.TotalMinutes; minutes += minimumResolution)
            {
                var duration = TimeSpan.FromMinutes(minutes);
                possibleOvertimeDurationsToCalculate.Add(duration);
            }

			var shiftEndingTime = scheduleDay.ProjectionService().CreateProjection().Period().GetValueOrDefault().EndDateTime;
			var max = scheduleDay.Period.StartDateTime.AddMinutes(overtimeSpecifiedPeriod.Maximum.TotalMinutes);

            var calculatedOvertimePeriods = new Dictionary<TimeSpan, double>();
            foreach (var duration in possibleOvertimeDurationsToCalculate)
            {
				if (duration < overtimeDurantion.Minimum) continue;
                var period = new DateTimePeriod(shiftEndingTime, shiftEndingTime.Add(duration));
				if(period.EndDateTime > max) continue;

	            if (onlyOvertimeAvaialbility)
				{
					var timeZoneInfo = scheduleDay.Person.PermissionInformation.DefaultTimeZone();
					var adjustedPeriods = _analyzePersonAccordingToAvailability.AdustOvertimeAvailability(scheduleDay, scheduleDay.DateOnlyAsPeriod.DateOnly, timeZoneInfo, new List<DateTimePeriod> { period });
					if(adjustedPeriods.Count == 0) continue;
					period = adjustedPeriods.First();
				}

	            if (!checkIfPeriodExistsInMappedData(overtimePeriodValueMappedData, period) || (calculatedOvertimePeriods.ContainsKey(period.ElapsedTime()))) continue;
                var sumOfRelativeDifference = calculateSumOfRelativeDifferencesForPeriod(period, overtimePeriodValueMappedData);
                calculatedOvertimePeriods.Add(period.ElapsedTime(), sumOfRelativeDifference);
            }

            var lowestRelativeDifferenceSum = double.MaxValue;
            var lowestValueDuration = TimeSpan.Zero;
            foreach (var duration in calculatedOvertimePeriods.Keys)
            {
                if (calculatedOvertimePeriods[duration] >= 0) continue;
                if (calculatedOvertimePeriods[duration] < lowestRelativeDifferenceSum)
                {
                    lowestRelativeDifferenceSum = calculatedOvertimePeriods[duration];
                    lowestValueDuration = duration;
                }
            }

			
			var dateTimePeriod = new DateTimePeriod(shiftEndingTime, shiftEndingTime.Add(lowestValueDuration));
			if(dateTimePeriod.ElapsedTime() == TimeSpan.Zero) return new List<DateTimePeriod>();

			IList<DateTimePeriod>  periods = new List<DateTimePeriod> { dateTimePeriod };
			return periods;
        }

        private bool checkIfPeriodExistsInMappedData(IEnumerable<OvertimePeriodValue> overtimePeriodValueMappedData, DateTimePeriod period)
        {
            foreach (var overtimePeriodValue in overtimePeriodValueMappedData)
            {
                if (period.EndDateTime <= overtimePeriodValue.Period.EndDateTime)
                    return true;
            }
            return false;
        }

        private double calculateSumOfRelativeDifferencesForPeriod(DateTimePeriod period, IEnumerable<OvertimePeriodValue> mappedData)
        {
            return mappedData.Where(x => period.Contains(x.Period))
                                                            .Sum(x => x.Value);

        }
    }
}