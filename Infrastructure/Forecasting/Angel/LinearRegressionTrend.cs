using System.Collections.Generic;
using MathNet.Numerics;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;

namespace Teleopti.Ccc.Infrastructure.Forecasting.Angel
{
	public class LinearRegressionTrend : ILinearRegressionTrend
	{

		public LinearTrend CalculateTrend(TaskOwnerPeriod historicalData)
		{
			var xlist = new List<double>();
			var ylist = new List<double>();
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				xlist.Add(taskOwner.CurrentDate.Subtract(LinearTrend.StartDate).Days);
				ylist.Add(taskOwner.TotalStatisticCalculatedTasks);
			}

			var tuple = Fit.Line(xlist.ToArray(), ylist.ToArray());
			return new LinearTrend
			{
				A = tuple.Item1,
				B = tuple.Item2
			};
		}
	}
}