﻿using System;
using Teleopti.Ccc.Domain.Forecasting.Angel;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class ForecastInput
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public Guid WorkloadId { get; set; }
		public Guid ScenarioId { get; set; }
	}
}