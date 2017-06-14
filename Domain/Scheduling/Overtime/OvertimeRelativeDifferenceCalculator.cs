using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeRelativeDifferenceCalculator
	{
		IEnumerable<IOvertimePeriodValue> Calculate(IEnumerable<DateTimePeriod> periods, IList<OvertimePeriodValue> overtimePeriodValueMappedData, bool onlyOvertimeAvaialbility, IScheduleDay scheduleDay);
	}

	public class OvertimeRelativeDifferenceCalculator : IOvertimeRelativeDifferenceCalculator
	{
		private readonly IAnalyzePersonAccordingToAvailability _analyzePersonAccordingToAvailability;

		public OvertimeRelativeDifferenceCalculator(IAnalyzePersonAccordingToAvailability analyzePersonAccordingToAvailability)
		{
			_analyzePersonAccordingToAvailability = analyzePersonAccordingToAvailability;
		}

		public IEnumerable<IOvertimePeriodValue> Calculate(IEnumerable<DateTimePeriod> periods, IList<OvertimePeriodValue> overtimePeriodValueMappedData, bool onlyOvertimeAvaialbility, IScheduleDay scheduleDay)
		{
			IList<IOvertimePeriodValue> possibleOvertimePeriods = new List<IOvertimePeriodValue>();
			
			foreach (var period in periods)
			{
				if (!overtimePeriodValueMappedData.Any(overtimePeriodValue => period.EndDateTime <= overtimePeriodValue.Period.EndDateTime)) break;

				if (onlyOvertimeAvaialbility)
				{
					var timeZoneInfo = scheduleDay.Person.PermissionInformation.DefaultTimeZone();
					var adjustedPeriod = _analyzePersonAccordingToAvailability.AdjustOvertimeAvailability(scheduleDay, scheduleDay.DateOnlyAsPeriod.DateOnly, timeZoneInfo, period);
					if (adjustedPeriod.HasValue)
					{
						calculateAndAddToPossiblePeriods(possibleOvertimePeriods, overtimePeriodValueMappedData, adjustedPeriod.Value);
					}
				}
				else
				{
					calculateAndAddToPossiblePeriods(possibleOvertimePeriods, overtimePeriodValueMappedData, period);
				}
			}

			return possibleOvertimePeriods;
		}

		private static void calculateAndAddToPossiblePeriods(IList<IOvertimePeriodValue> possibleOvertimePeriods, IList<OvertimePeriodValue> overtimePeriodValueMappedData, DateTimePeriod period)
		{
			period = adjustedToMappedData(period, overtimePeriodValueMappedData);
			var sumOfRelativeDifference = overtimePeriodValueMappedData.Where(x => period.Contains(x.Period)).Sum(x => x.Value);
			possibleOvertimePeriods.Add(new OvertimePeriodValue(period, sumOfRelativeDifference));
		}

		private static DateTimePeriod adjustedToMappedData(DateTimePeriod period, IList<OvertimePeriodValue> overtimePeriodValueMappedData)
		{
			if (!overtimePeriodValueMappedData.Any(x => x.Period.Contains(period.EndDateTime)))
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
