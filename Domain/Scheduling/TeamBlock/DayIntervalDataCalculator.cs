using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
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
			if (dayIntervalData == null) return null;
			InParameter.ValueMustBeLargerThanZero("resolution", resolution);

			var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);
			var result = new Dictionary<TimeSpan, ISkillIntervalData>();
			var intervalsOfTwoDays = new List<TimeSpan>();
			for (var i = 0; i < TimeSpan.FromDays(2).TotalMinutes / resolution; i++)
			{
				intervalsOfTwoDays.Add(TimeSpan.FromMinutes(i * resolution));
			}
			
			foreach (var interval in intervalsOfTwoDays)
			{
				var forecastedDemands = new List<double>();
				var currentDemands = new List<double>();
			    var minimumStaff = new List<double>();
			    var maximumStaff = new List<double>();
			    var calculatedLoggedOnStaff = new List<double>();
				foreach (var dayIntervalPair in dayIntervalData)
				{
					TimeSpan interval1 = interval;
					var dataInInteral = dayIntervalPair.Value.FirstOrDefault(x => x.Period.StartDateTime.TimeOfDay == interval1);
					if (dataInInteral != null)
					{
						forecastedDemands.Add(dataInInteral.ForecastedDemand);
						currentDemands.Add(dataInInteral.CurrentDemand);
                        if (dataInInteral.MinimumHeads.HasValue ) 
							minimumStaff.Add(dataInInteral.MinimumHeads.Value );
                        if (dataInInteral.MaximumHeads.HasValue ) 
							maximumStaff.Add(dataInInteral.MaximumHeads.Value );
                        calculatedLoggedOnStaff.Add(dataInInteral.CurrentHeads);

					}
				}
				if(forecastedDemands.Count == 0) continue;
			    var calculatedFDemand = _intervalDataCalculator.Calculate(forecastedDemands);
				var calculatedCDemand = _intervalDataCalculator.Calculate(currentDemands);
			    var calculatedMinimumStaff = _intervalDataCalculator.Calculate(minimumStaff);
			    var calculatedMaximumStaff = _intervalDataCalculator.Calculate(maximumStaff);
                var calculatedLoggedOn = _intervalDataCalculator.Calculate(calculatedLoggedOnStaff);
				var startTime = baseDate.Date.Add(interval);
				var endTime = startTime.AddMinutes(resolution);
                ISkillIntervalData skillIntervalData = new SkillIntervalData(new DateTimePeriod(startTime, endTime), calculatedFDemand, calculatedCDemand, calculatedLoggedOn, calculatedMinimumStaff, calculatedMaximumStaff);
				result.Add(interval, skillIntervalData);
			}
			return result;
		}
	}
}
