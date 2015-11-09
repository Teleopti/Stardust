using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillStatusMeasure
	{
		public string Name { get; set; }
		public double Value { get; set; }
		public double ForecastedCalls { get; set; }
		public double ActualCalls { get; set; }
		public string StringValue { get; set; }
		public int Severity { get; set; }
		public DateTime LatestDate { get; set; }
	}
}