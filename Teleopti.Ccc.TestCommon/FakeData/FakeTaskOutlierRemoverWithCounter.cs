using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeTaskOutlierRemoverWithCounter : IOutlierRemover
	{
		public ITaskOwnerPeriod RemoveOutliers(ITaskOwnerPeriod historicalData, IForecastMethod forecastMethod)
		{
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				if(taskOwner.TotalStatisticAverageTaskTime.TotalSeconds == 400d)
					((ValidatedVolumeDay)taskOwner).ValidatedAverageTaskTime = TimeSpan.FromSeconds(100d);
			}

			return historicalData;
		}
	}
}