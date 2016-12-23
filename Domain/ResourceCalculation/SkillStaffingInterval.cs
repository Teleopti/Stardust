using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillStaffingInterval : IResourceCalculationPeriod, IShovelResourceDataForInterval
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

		public double ForecastedDistributedDemand => Forecast;

		public void SetCalculatedResource65(double resources)
		{
			StaffingLevel = resources;
		}

		public void SetCalculatedLoggedOn(double loggedOn)
		{
			
		}

		public void ResetMultiskillMinOccupancy()
		{
			
		}

		public void AddResources(double resourcesToAdd)
		{
			var newValue = Math.Max(0, StaffingLevel + resourcesToAdd);
			StaffingLevel = newValue;
		}

		public double AbsoluteDifference => CalculatedResource - FStaff;
		public double RelativeDifference => new DeviationStatisticData(FStaff, CalculatedResource).RelativeDeviation;
		public double CalculatedResource { get; set; }
		public double FStaff { get; set; }
	}
}