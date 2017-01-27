﻿using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class FakeMissingForecastProvider : IMissingForecastProvider
	{
		public MissingForecastModel[] MissingForecast { get; set; }

		public IEnumerable<MissingForecastModel> GetMissingForecast(DateOnlyPeriod range)
		{
			return MissingForecast ?? new MissingForecastModel[0];
		}
	}
}