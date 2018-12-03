using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class CalculateBestOvertimeBeforeOrAfter
	{
		private readonly IOvertimeDateTimePeriodExtractor _overtimeDateTimePeriodExtractor;
		private readonly OvertimeRelativeDifferenceCalculator _overtimeRelativeDifferenceCalculator;
		private readonly OvertimePeriodValueMapper _overtimePeriodValueMapper;

		public CalculateBestOvertimeBeforeOrAfter(IOvertimeDateTimePeriodExtractor overtimeDateTimePeriodExtractor, OvertimeRelativeDifferenceCalculator overtimeRelativeDifferenceCalculator,
												  OvertimePeriodValueMapper overtimePeriodValueMapper)
		{
			_overtimeDateTimePeriodExtractor = overtimeDateTimePeriodExtractor;
			_overtimeRelativeDifferenceCalculator = overtimeRelativeDifferenceCalculator;
			_overtimePeriodValueMapper = overtimePeriodValueMapper;
		}

		public IEnumerable<DateTimePeriod> GetBestOvertimeInUtc(MinMax<TimeSpan> overtimeDuration, DateTimePeriod specifiedPeriod, IScheduleRange scheduleRange, DateOnly date, int minimumResolution, bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			var overtimePeriodValueMappedData = _overtimePeriodValueMapper.Map(skillIntervalDataList);
			var scheduleDay = scheduleRange.ScheduledDay(date);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var personAssignment = scheduleDay.PersonAssignment();
			if (personAssignment == null) return new List<DateTimePeriod>();
			var result = _overtimeDateTimePeriodExtractor.Extract(minimumResolution, overtimeDuration, visualLayerCollection, specifiedPeriod, skillIntervalDataList);
			var possibleOvertimePeriods = _overtimeRelativeDifferenceCalculator.Calculate(result, overtimePeriodValueMappedData, onlyAvailableAgents, scheduleRange, date);
			var sortedPeriodValues = possibleOvertimePeriods.Where(possibleOvertimePeriod => possibleOvertimePeriod.Value < 0).OrderBy(x => x.Value);

			return sortedPeriodValues.Select(overtimePeriodValue => overtimePeriodValue.Period);
		}

		public IEnumerable<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDuration, MinMax<TimeSpan> overtimeSpecifiedPeriod, IScheduleRange scheduleRange, DateOnly date, int minimumResolution, bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			var overtimePeriodValueMappedData = _overtimePeriodValueMapper.Map(skillIntervalDataList);
			var scheduleDay = scheduleRange.ScheduledDay(date);
			var scheduleDayPeriodStart = scheduleDay.Period.StartDateTime;
			var start = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Minimum);
			var end = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Maximum);
			var specifiedPeriod = new DateTimePeriod(start, end);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var result = _overtimeDateTimePeriodExtractor.Extract(minimumResolution, overtimeDuration, visualLayerCollection, specifiedPeriod, skillIntervalDataList);
			var possibleOvertimePeriods = _overtimeRelativeDifferenceCalculator.Calculate(result, overtimePeriodValueMappedData, onlyAvailableAgents, scheduleRange, date);
			var sortedPeriodValues = possibleOvertimePeriods.Where(possibleOvertimePeriod => possibleOvertimePeriod.Value < 0).OrderBy(x => x.Value);

			return sortedPeriodValues.Select(overtimePeriodValue => overtimePeriodValue.Period);
		}
	}
}
