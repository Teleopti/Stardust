﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportModel
	{
		public ForecastExportHeader Header { get; set; }
		public List<DailyModelForecast> DailyModelForecast { get; set; }
	}

	public class ForecastExportHeader
	{
		public DateOnlyPeriod Period { get; set; }
		public string SkillName { get; set; }
		public string SkillTimeZoneName { get; set; }
		public Percent ServiceLevelPercent { get; set; }
		public double ServiceLevelSeconds { get; set; }
		public Percent ShrinkagePercent { get; set; }
	}
}