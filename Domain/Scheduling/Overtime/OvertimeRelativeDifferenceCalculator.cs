using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class OvertimeRelativeDifferenceCalculator
	{
		private readonly AnalyzePersonAccordingToAvailability _analyzePersonAccordingToAvailability;

		public OvertimeRelativeDifferenceCalculator(AnalyzePersonAccordingToAvailability analyzePersonAccordingToAvailability)
		{
			_analyzePersonAccordingToAvailability = analyzePersonAccordingToAvailability;
		}

		public virtual IEnumerable<IOvertimePeriodValue> Calculate(IEnumerable<DateTimePeriod> periods, IEnumerable<OvertimePeriodValue> overtimePeriodValueMappedData, bool onlyOvertimeAvailability, IScheduleRange scheduleRange, DateOnly date)
		{
			var possibleOvertimePeriods = new List<IOvertimePeriodValue>();

			foreach (var period in periods)
			{
				if (!overtimePeriodValueMappedData.Any(overtimePeriodValue => period.EndDateTime <= overtimePeriodValue.Period.EndDateTime)) break;

				if (onlyOvertimeAvailability)
				{
					possiblePeriodsOvertimeAvailability(overtimePeriodValueMappedData, scheduleRange, date, period, possibleOvertimePeriods);
					possiblePeriodsOvertimeAvailability(overtimePeriodValueMappedData, scheduleRange, date.AddDays(1), period, possibleOvertimePeriods);
				}
				else
				{
					calculateAndAddToPossiblePeriods(possibleOvertimePeriods, overtimePeriodValueMappedData, period);
				}
			}

			return possibleOvertimePeriods;
		}

		private void possiblePeriodsOvertimeAvailability(IEnumerable<OvertimePeriodValue> overtimePeriodValueMappedData, IScheduleRange scheduleRange, DateOnly firstDate, DateTimePeriod period, ICollection<IOvertimePeriodValue> possibleOvertimePeriods)
		{
			var scheduleDay = scheduleRange.ScheduledDay(firstDate);
			var adjustedPeriod = _analyzePersonAccordingToAvailability.AdjustOvertimeAvailability(scheduleDay, period);
			if (adjustedPeriod.HasValue)
			{
				calculateAndAddToPossiblePeriods(possibleOvertimePeriods, overtimePeriodValueMappedData, adjustedPeriod.Value);
			}
		}

		private void calculateAndAddToPossiblePeriods(ICollection<IOvertimePeriodValue> possibleOvertimePeriods, IEnumerable<OvertimePeriodValue> overtimePeriodValueMappedData, DateTimePeriod period)
		{
			period = adjustedToMappedData(period, overtimePeriodValueMappedData);
			var sumOfRelativeDifference = overtimePeriodValueMappedData.Where(x => period.Contains(x.Period)).Sum(x => x.Value);
			possibleOvertimePeriods.Add(new OvertimePeriodValue(period, sumOfRelativeDifference));
		}

		private static DateTimePeriod adjustedToMappedData(DateTimePeriod period, IEnumerable<OvertimePeriodValue> overtimePeriodValueMappedData)
		{
			if (!overtimePeriodValueMappedData.Any(x => x.Period.ContainsPart(period.EndDateTime)))
			{
				var last = overtimePeriodValueMappedData.LastOrDefault(x => period.Contains(x.Period));
				if(last != null)
					return new DateTimePeriod(period.StartDateTime, last.Period.EndDateTime);
			}

			if (!overtimePeriodValueMappedData.Any(x => x.Period.Contains(period.StartDateTime)))
			{
				var first = overtimePeriodValueMappedData.FirstOrDefault(x => period.Contains(x.Period));
				if (first != null)
					return new DateTimePeriod(first.Period.StartDateTime, period.EndDateTime);
			}

			return period;
		}
	}
}
