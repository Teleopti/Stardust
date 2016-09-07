using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface ICalculateBestOvertime
	{
		IEnumerable<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDuration, MinMax<TimeSpan> overtimeSpecifiedPeriod,
			IList<OvertimePeriodValue> overtimePeriodValueMappedDat, IScheduleDay scheduleDay, int minimumResolution,
			bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList);
	}


	public class CalculateBestOvertimeBeforeOrAfter : ICalculateBestOvertime
	{
		private readonly IOvertimeDateTimePeriodExtractor _overtimeDateTimePeriodExtractor;
		private readonly IOvertimeRelativeDifferenceCalculator _overtimeRelativeDifferenceCalculator;

		public CalculateBestOvertimeBeforeOrAfter(IOvertimeDateTimePeriodExtractor overtimeDateTimePeriodExtractor, IOvertimeRelativeDifferenceCalculator overtimeRelativeDifferenceCalculator)
		{
			_overtimeDateTimePeriodExtractor = overtimeDateTimePeriodExtractor;
			_overtimeRelativeDifferenceCalculator = overtimeRelativeDifferenceCalculator;
		}

		public IEnumerable<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDuration, MinMax<TimeSpan> overtimeSpecifiedPeriod, IList<OvertimePeriodValue> overtimePeriodValueMappedData, IScheduleDay scheduleDay, int minimumResolution, bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			var scheduleDayPeriodStart = scheduleDay.Period.StartDateTime;
			var start = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Minimum);
			var end = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Maximum);
			var specifiedPeriod = new DateTimePeriod(start, end);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var result = _overtimeDateTimePeriodExtractor.Extract(minimumResolution, overtimeDuration, visualLayerCollection, specifiedPeriod, skillIntervalDataList);
			var possibleOvertimePeriods = _overtimeRelativeDifferenceCalculator.Calculate(result, overtimePeriodValueMappedData, onlyAvailableAgents, scheduleDay);
			var sortedPeriodValues = possibleOvertimePeriods.Where(possibleOvertimePeriod => possibleOvertimePeriod.Value < 0).ToList().OrderBy(x => x.Value);

			return sortedPeriodValues.Select(overtimePeriodValue => overtimePeriodValue.Period).ToList();
		}
	}
}
