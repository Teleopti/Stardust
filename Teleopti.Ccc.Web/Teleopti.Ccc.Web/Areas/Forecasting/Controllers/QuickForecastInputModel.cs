﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class QuickForecastInputModel
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
	}
}