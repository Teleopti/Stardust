using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillStaffingInterval
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Forecast { get; set; }
		public double StaffingLevel { get; set; }
		public double ForecastWithShrinkage { get; set; }

		public double StaffingLevelWithShrinkage { get; set; }

		public double GetForecast(bool withShrinkage)
		{
			return withShrinkage ? ForecastWithShrinkage : Forecast;
		}

		public double GetStaffingLevel(bool withShrinkage)
		{
			return withShrinkage ? StaffingLevelWithShrinkage : StaffingLevel;
		}

		public TimeSpan GetTimeSpan()
		{
			return EndDateTime.Subtract(StartDateTime);
		}

		public double DivideBy(TimeSpan ts)
		{
			return (double)GetTimeSpan().Ticks / ts.Ticks;
		}
	}
}