using System.Collections.Generic;
using MathNet.Numerics;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;

namespace Teleopti.Ccc.Infrastructure.Forecasting.Angel
{
	public class LinearRegressionTrend : ILinearRegressionTrend
	{

		public LinearTrend CalculateTrend(ITaskOwnerPeriod historicalData)
		{
			var xlist = new List<double>();
			var ylist = new List<double>();
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				if (taskOwner.TotalStatisticCalculatedTasks > 0)
				{
					xlist.Add(taskOwner.CurrentDate.Subtract(LinearTrend.StartDate).Days);
					ylist.Add(taskOwner.TotalStatisticCalculatedTasks);
				}
			}

			if (xlist.Count < 2 || ylist.Count < 2)
				return LinearTrend.NoLinearTrend;

			var tuple = Fit.Line(xlist.ToArray(), ylist.ToArray());
			return new LinearTrend
			{
				Slope = tuple.Item2,
				Intercept = tuple.Item1
			};
		}
	}
}