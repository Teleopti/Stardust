﻿using System;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class QueueStatisticsInput
	{
		public Guid WorkloadId { get; set; }
		public ForecastMethodType MethodId { get; set; }
	}
}