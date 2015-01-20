using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class CalculateBestOvertimeBeforeOrAfter : ICalculateBestOvertime
	{
		private readonly IOvertimeDateTimePeriodExtractor _overtimeDateTimePeriodExtractor;
		private readonly IOvertimeRelativeDifferenceCalculator _overtimeRelativeDifferenceCalculator;

		public CalculateBestOvertimeBeforeOrAfter(IOvertimeDateTimePeriodExtractor overtimeDateTimePeriodExtractor, IOvertimeRelativeDifferenceCalculator overtimeRelativeDifferenceCalculator)
		{
			_overtimeDateTimePeriodExtractor = overtimeDateTimePeriodExtractor;
			_overtimeRelativeDifferenceCalculator = overtimeRelativeDifferenceCalculator;
		}

		public IList<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDuration, MinMax<TimeSpan> overtimeSpecifiedPeriod, IList<OvertimePeriodValue> overtimePeriodValueMappedData,
			IScheduleDay scheduleDay, int minimumResolution, bool onlyAvailableAgents, IEnumerable<DateTimePeriod> openHoursList)
		{
			var scheduleDayPeriodStart = scheduleDay.Period.StartDateTime;
			var start = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Minimum);
			var end = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Maximum);

			var specifiedPeriod = new DateTimePeriod(start, end);

			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var result = _overtimeDateTimePeriodExtractor.Extract(minimumResolution, overtimeDuration, visualLayerCollection, specifiedPeriod, openHoursList);
			var possibleOvertimePeriods = _overtimeRelativeDifferenceCalculator.Calculate(result, overtimePeriodValueMappedData, onlyAvailableAgents, scheduleDay);
			var lowestRelativeDifferenceSum = double.MaxValue;
			IOvertimePeriodValues lowestPeriodValues = new OvertimePeriodValues();
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

	}
}
