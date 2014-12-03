using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class CalculateBestOvertimeBeforeOrAfter : ICalculateBestOvertime
	{
		private readonly IAnalyzePersonAccordingToAvailability _analyzePersonAccordingToAvailability;

		public CalculateBestOvertimeBeforeOrAfter(IAnalyzePersonAccordingToAvailability analyzePersonAccordingToAvailability)
		{
			_analyzePersonAccordingToAvailability = analyzePersonAccordingToAvailability;
		}

		public IList<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDurantion, IList<OvertimePeriodValue> overtimePeriodValueMappedData, IScheduleDay scheduleDay, int minimumResolution, bool onlyOvertimeAvaialbility)
		{
			var shiftPeriod = scheduleDay.ProjectionService().CreateProjection().Period().GetValueOrDefault();
			var shiftStart = shiftPeriod.StartDateTime;
			var shiftEnd = shiftPeriod.EndDateTime;
			var possibleOvertimePeriods = new List<OvertimePeriodValues>();
			
			for (var minutes = minimumResolution; minutes <= overtimeDurantion.Maximum.TotalMinutes; minutes += minimumResolution)
			{
				var duration = TimeSpan.FromMinutes(minutes);
				if(duration < overtimeDurantion.Minimum) continue;

				var beforePeriod = new DateTimePeriod(shiftStart.Add(-duration), shiftStart);
				var afterPeriod = new DateTimePeriod(shiftEnd, shiftEnd.Add(duration));

				calculateRelativeDifference(beforePeriod, overtimePeriodValueMappedData, possibleOvertimePeriods, onlyOvertimeAvaialbility, scheduleDay);
				calculateRelativeDifference(afterPeriod, overtimePeriodValueMappedData, possibleOvertimePeriods, onlyOvertimeAvaialbility, scheduleDay);
			}

			var lowestRelativeDifferenceSum = double.MaxValue;
			var lowestPeriodValues = new OvertimePeriodValues();

			foreach (var overtimePeriodValues in possibleOvertimePeriods)
			{
				var totalValue = overtimePeriodValues.TotalValue();
				if (totalValue >= 0) continue;
				if (!(totalValue < lowestRelativeDifferenceSum)) continue;
				lowestRelativeDifferenceSum = totalValue;
				lowestPeriodValues = overtimePeriodValues;
			}

			return lowestPeriodValues.PeriodValues.Select(overtimePeriodValue => overtimePeriodValue.Period).ToList();
		}

		private void calculateRelativeDifference(DateTimePeriod period, IList<OvertimePeriodValue> overtimePeriodValueMappedData, IList<OvertimePeriodValues> possibleOvertimePeriods, bool onlyOvertimeAvaialbility, IScheduleDay scheduleDay)
		{
			if (!overtimePeriodValueMappedData.Any(overtimePeriodValue => period.EndDateTime <= overtimePeriodValue.Period.EndDateTime)) return;

			if (onlyOvertimeAvaialbility)
			{
				var timeZoneInfo = scheduleDay.Person.PermissionInformation.DefaultTimeZone();
				var adjustedPeriods = _analyzePersonAccordingToAvailability.AdustOvertimeAvailability(scheduleDay, scheduleDay.DateOnlyAsPeriod.DateOnly, timeZoneInfo, new List<DateTimePeriod> { period });
				if (adjustedPeriods.Count == 0) return;
				period = adjustedPeriods.First();
			}

			var sumOfRelativeDifference = overtimePeriodValueMappedData.Where(x => period.Contains(x.Period)).Sum(x => x.Value);
			var overtimePeriodValues = new OvertimePeriodValues();
			overtimePeriodValues.Add(new OvertimePeriodValue(period, sumOfRelativeDifference));
			possibleOvertimePeriods.Add(overtimePeriodValues);	
		}
	}
}
