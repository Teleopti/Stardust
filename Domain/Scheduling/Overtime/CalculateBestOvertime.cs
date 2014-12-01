using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface ICalculateBestOvertime
    {
        List<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDurantion, IList<OvertimePeriodValue> overtimePeriodValueMappedDat, IScheduleDay scheduleDay, int minimumResolution);
    }

    public class CalculateBestOvertime : ICalculateBestOvertime
    {

		public List<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDurantion, IList<OvertimePeriodValue> overtimePeriodValueMappedData, IScheduleDay scheduleDay, int minimumResolution)
        {
            var possibleOvertimeDurationsToCalculate = new List<TimeSpan>();
            for (int minutes = minimumResolution; minutes <= overtimeDurantion.Maximum.TotalMinutes; minutes += minimumResolution)
            {
                var duration = TimeSpan.FromMinutes(minutes);
                possibleOvertimeDurationsToCalculate.Add(duration);
            }

			var shiftEndingTime = scheduleDay.ProjectionService().CreateProjection().Period().GetValueOrDefault().EndDateTime;

            var calculatedOvertimePeriods = new Dictionary<TimeSpan, double>();
            foreach (var duration in possibleOvertimeDurationsToCalculate)
            {
                var period = new DateTimePeriod(shiftEndingTime, shiftEndingTime.Add(duration));
                if (!checkIfPeriodExistsInMappedData(overtimePeriodValueMappedData, period)) continue;
                var sumOfRelativeDifference = calculateSumOfRelativeDifferencesForPeriod(period, overtimePeriodValueMappedData);
                calculatedOvertimePeriods.Add(duration, sumOfRelativeDifference);
            }
            var lowestRelativeDifferenceSum = double.MaxValue;
            var lowestValueDuration = TimeSpan.Zero;
            foreach (var duration in calculatedOvertimePeriods.Keys)
            {
                if (duration < overtimeDurantion.Minimum) continue;
                if (calculatedOvertimePeriods[duration] >= 0) continue;
                if (calculatedOvertimePeriods[duration] < lowestRelativeDifferenceSum)
                {
                    lowestRelativeDifferenceSum = calculatedOvertimePeriods[duration];
                    lowestValueDuration = duration;
                }
            }

			var dateTimePeriod = new DateTimePeriod(shiftEndingTime, shiftEndingTime.Add(lowestValueDuration));
			return new List<DateTimePeriod> { dateTimePeriod };
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