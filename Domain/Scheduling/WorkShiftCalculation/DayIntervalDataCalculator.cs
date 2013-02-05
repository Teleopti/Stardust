using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface IDayIntervalDataCalculator
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<TimeSpan, ISkillIntervalData> Calculate(int resolution, IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
	}

	public class DayIntervalDataCalculator : IDayIntervalDataCalculator
	{
		private readonly IIntervalDataCalculator _intervalDataCalculator;

		public DayIntervalDataCalculator(IIntervalDataCalculator intervalDataCalculator)
		{
			_intervalDataCalculator = intervalDataCalculator;
		}

		public IDictionary<TimeSpan, ISkillIntervalData> Calculate(int resolution, IDictionary<DateOnly,
			IList<ISkillIntervalData>> dayIntervalData)
		{
			InParameter.NotNull("dayIntervalData", dayIntervalData);
			InParameter.ValueMustBeLargerThanZero("resolution", resolution);

			var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);
			var result = new Dictionary<TimeSpan, ISkillIntervalData>();
			var intervalBasedData = new Dictionary<TimeSpan, List<double>>();
			for (var i = 0; i < TimeSpan.FromDays(2).TotalMinutes / resolution; i++)
			{
				intervalBasedData.Add(TimeSpan.FromMinutes(i * resolution), new List<double>());
			}
			foreach (var intervalData in intervalBasedData)
			{
				var intervalTimeSpan = intervalData.Key;
				var intervals = new List<double>();
			    if (dayIntervalData != null)
			        foreach (var keyValuePair in dayIntervalData)
			        {
			            var oneInterval = keyValuePair.Value.FirstOrDefault(x => x.Period.StartDateTime.TimeOfDay == intervalTimeSpan);
			            if (oneInterval != null)
			                intervals.Add(oneInterval.ForecastedDemand - oneInterval.CurrentDemand);
			        }
			    var calculatedDemand = _intervalDataCalculator.Calculate(intervals);
				var startTime = baseDate.Date.Add(intervalTimeSpan);
				var endTime = startTime.AddMinutes(resolution);
				ISkillIntervalData skillIntervalData = new SkillIntervalData(new DateTimePeriod(startTime, endTime),
																			 calculatedDemand, 0, 0, 0, 0);
				result.Add(intervalTimeSpan, skillIntervalData);
			}
			return result;
		}
	}
}
