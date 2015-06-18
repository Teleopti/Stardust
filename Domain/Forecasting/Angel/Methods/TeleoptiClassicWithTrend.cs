using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class TeleoptiClassicWithTrend : TeleoptiClassicUpdatedBase
	{
		private readonly ILinearTrendCalculator _linearTrendCalculator;

		public TeleoptiClassicWithTrend(IIndexVolumes indexVolumes, ILinearTrendCalculator linearTrendCalculator)
			: base(indexVolumes, new SimpleAhtAndAcwCalculator())
		{
			_linearTrendCalculator = linearTrendCalculator;
		}

		protected override IEnumerable<DateAndTask> ForecastNumberOfTasks(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var trend = _linearTrendCalculator.CalculateTrend(historicalData);
			var dateAndTasks = base.ForecastNumberOfTasks(historicalData, futurePeriod).ToArray();

			var averageTasks = dateAndTasks.Average(x => x.Tasks);
			foreach (var dateAndTask in dateAndTasks)
			{
				dateAndTask.Tasks = Math.Max(0, dateAndTask.Tasks + dateAndTask.Date.Subtract(LinearTrend.StartDate).Days * trend.Slope + trend.Intercept - averageTasks);
			}
			return dateAndTasks;

		}

		public override ForecastMethodType Id {
			get
			{
				return ForecastMethodType.TeleoptiClassicWithTrend;
			}
		}
	}
}