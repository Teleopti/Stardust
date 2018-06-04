using System;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeForecastViewModelFactory: IForecastViewModelFactory
	{
		public WorkloadEvaluateViewModel Evaluate(EvaluateInput input)
		{
			throw new NotImplementedException();
		}

		public WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input)
		{
			throw new NotImplementedException();
		}

		public WorkloadEvaluateMethodsViewModel EvaluateMethods(EvaluateMethodsInput input)
		{
			throw new NotImplementedException();
		}
	}
}
