using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	/// <summary>
	/// http://en.wikipedia.org/wiki/Mean_absolute_percentage_error
	/// </summary>
	public class ForecastingMeanAbsolutePercentageDeviation : IForecastingMeasurer
	{
		public double Measure(IList<IForecastingTarget> forecastingForLastYear, ReadOnlyCollection<ITaskOwner> historicalDataForLastYear)
		{
			var diffSum = 0d;
			var numberOfSkipped = 0;
			foreach (var day in historicalDataForLastYear)
			{
				var diff = 0d;
				foreach (var forecastingDay in forecastingForLastYear)
				{
					if (forecastingDay.CurrentDate == day.CurrentDate)
					{
						if (Math.Abs(day.TotalStatisticCalculatedTasks) < 0.000001 && Math.Abs(forecastingDay.Tasks) < 0.000001)
						{
							diff = 0d;
						}
						else if (Math.Abs(day.TotalStatisticCalculatedTasks) < 0.000001)
						{
							// a main drawback in this measurement method
							numberOfSkipped++;
						}
						else
						{
							diff = Math.Abs(day.TotalStatisticCalculatedTasks - forecastingDay.Tasks) / day.TotalStatisticCalculatedTasks;
						}
						break;
					}
				}

				diffSum += diff;
			}
			if (historicalDataForLastYear.Count - numberOfSkipped == 0)
			{
				return Double.NaN;
			}
			return Math.Max(0, 100 - Math.Round(diffSum/(historicalDataForLastYear.Count - numberOfSkipped)*100, 3));
		}
	}
}