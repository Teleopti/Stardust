using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	/// <summary>
	/// http://en.wikipedia.org/wiki/Symmetric_mean_absolute_percentage_error
	/// </summary>
	public class ForecastingSymmetricMeanAbsolutePercentageError : IForecastingMeasurer
	{
		public double Measure(IList<IForecastingTarget> forecastingForLastYear, ReadOnlyCollection<ITaskOwner> historicalDataForLastYear)
		{
			var diffSum = 0d;
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
						else
						{
							diff = Math.Abs(day.TotalStatisticCalculatedTasks - forecastingDay.Tasks) / (Math.Abs(day.TotalStatisticCalculatedTasks) + Math.Abs(forecastingDay.Tasks)) * 2;
						}
						break;
					}
				}

				diffSum += diff;
			}
			return Math.Max(0, 100 - Math.Round(diffSum / historicalDataForLastYear.Count * 100, 3));
		}
	}
}