﻿using System;
using Teleopti.Ccc.Domain.Forecasting.Angel;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class ForecastInput
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public ForecastWorkloadInput[] Workloads { get; set; }
	}

	public class ForecastResultInput
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public Guid WorkloadId { get; set; }
	}

	public class EvaluateMethodsInput
	{
		public Guid WorkloadId { get; set; }
	}

	public class IntradayPatternInput
	{
		public Guid WorkloadId { get; set; }
	}
}