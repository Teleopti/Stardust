using System.Collections.Generic;
using System.Linq;
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
					
				var adjustedPeriod = period;
				if (onlyOvertimeAvaialbility)
				{
					var timeZoneInfo = scheduleDay.Person.PermissionInformation.DefaultTimeZone();
					var adjustedPeriods = _analyzePersonAccordingToAvailability.AdustOvertimeAvailability(scheduleDay, scheduleDay.DateOnlyAsPeriod.DateOnly, timeZoneInfo, new List<DateTimePeriod> { period });
					if (adjustedPeriods.Count == 0) break;
						
					adjustedPeriod = adjustedPeriods.First();
				}

				var sumOfRelativeDifference = overtimePeriodValueMappedData.Where(x => adjustedPeriod.Contains(x.Period)).Sum(x => x.Value);
				possibleOvertimePeriods.Add(new OvertimePeriodValue(adjustedPeriod, sumOfRelativeDifference));
			}

			return possibleOvertimePeriods;
		}
	}
}
