using System.Collections.Generic;
using MathNet.Numerics;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Trend
{
	public class LinearRegressionTrendCalculator : ILinearTrendCalculator
	{
		public LinearTrend CalculateTrend(ITaskOwnerPeriod historicalData)
		{
			var xList = new List<double>();
			var yList = new List<double>();
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				if (!(taskOwner.TotalStatisticCalculatedTasks > 0)) continue;

				xList.Add(taskOwner.CurrentDate.Subtract(LinearTrend.StartDate).Days);
				yList.Add(taskOwner.TotalStatisticCalculatedTasks);
			}

			if (xList.Count < 2 || yList.Count < 2)
				return LinearTrend.NoLinearTrend;

			var tuple = Fit.Line(xList.ToArray(), yList.ToArray());
			return new LinearTrend
			{
				Slope = tuple.Item2,
				Intercept = tuple.Item1
			};
		}
	}
}