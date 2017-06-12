using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface ICalculateBestOvertime
	{
		IEnumerable<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDuration, MinMax<TimeSpan> overtimeSpecifiedPeriod, IScheduleDay scheduleDay, int minimumResolution,
			bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList);

		IEnumerable<DateTimePeriod> GetBestOvertimeInUtc(MinMax<TimeSpan> overtimeDuration, DateTimePeriod specifiedPeriod, IScheduleDay scheduleDay, int minimumResolution,
			bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList);
	}


	public class CalculateBestOvertimeBeforeOrAfter : ICalculateBestOvertime
	{
		private readonly IOvertimeDateTimePeriodExtractor _overtimeDateTimePeriodExtractor;
		private readonly IOvertimeRelativeDifferenceCalculator _overtimeRelativeDifferenceCalculator;
		private readonly IOvertimePeriodValueMapper _overtimePeriodValueMapper;

		public CalculateBestOvertimeBeforeOrAfter(IOvertimeDateTimePeriodExtractor overtimeDateTimePeriodExtractor, IOvertimeRelativeDifferenceCalculator overtimeRelativeDifferenceCalculator,
												  IOvertimePeriodValueMapper overtimePeriodValueMapper)
		{
			_overtimeDateTimePeriodExtractor = overtimeDateTimePeriodExtractor;
			_overtimeRelativeDifferenceCalculator = overtimeRelativeDifferenceCalculator;
			_overtimePeriodValueMapper = overtimePeriodValueMapper;
		}

		public IEnumerable<DateTimePeriod> GetBestOvertimeInUtc(MinMax<TimeSpan> overtimeDuration, DateTimePeriod specifiedPeriod, IScheduleDay scheduleDay, int minimumResolution, bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			var overtimePeriodValueMappedData = _overtimePeriodValueMapper.Map(skillIntervalDataList);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var personAssignment = scheduleDay.PersonAssignment();
			if (personAssignment == null) return new List<DateTimePeriod>();
			var result = _overtimeDateTimePeriodExtractor.Extract(minimumResolution, overtimeDuration, visualLayerCollection, specifiedPeriod, skillIntervalDataList);
			var possibleOvertimePeriods = _overtimeRelativeDifferenceCalculator.Calculate(result, overtimePeriodValueMappedData, onlyAvailableAgents, scheduleDay);
			var sortedPeriodValues = possibleOvertimePeriods.Where(possibleOvertimePeriod => possibleOvertimePeriod.Value < 0).OrderBy(x => x.Value);

			return sortedPeriodValues.Select(overtimePeriodValue => overtimePeriodValue.Period);
		}

		public IEnumerable<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDuration, MinMax<TimeSpan> overtimeSpecifiedPeriod, IScheduleDay scheduleDay, int minimumResolution, bool onlyAvailableAgents, IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			var overtimePeriodValueMappedData = _overtimePeriodValueMapper.Map(skillIntervalDataList);
			var scheduleDayPeriodStart = scheduleDay.Period.StartDateTime;
			var start = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Minimum);
			var end = scheduleDayPeriodStart.Add(overtimeSpecifiedPeriod.Maximum);
			var specifiedPeriod = new DateTimePeriod(start, end);
			var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
			var result = _overtimeDateTimePeriodExtractor.Extract(minimumResolution, overtimeDuration, visualLayerCollection, specifiedPeriod, skillIntervalDataList);
			var possibleOvertimePeriods = _overtimeRelativeDifferenceCalculator.Calculate(result, overtimePeriodValueMappedData, onlyAvailableAgents, scheduleDay);
			var sortedPeriodValues = possibleOvertimePeriods.Where(possibleOvertimePeriod => possibleOvertimePeriod.Value < 0).OrderBy(x => x.Value);

			return sortedPeriodValues.Select(overtimePeriodValue => overtimePeriodValue.Period);
		}
	}
}
