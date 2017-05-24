using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillStaffingInterval : IResourceCalculationPeriod, IShovelResourceDataForInterval, IValidatePeriod
	{
		private double _loggedOn;
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }

		public DateTimePeriod CalculationPeriod => new DateTimePeriod(StartDateTime, EndDateTime);
		public double CalculatedLoggedOn => _loggedOn;

		public void SetCalculatedUsedSeats(double usedSeats)
		{
		}

		public double Forecast { get; set; }
		public double StaffingLevel { get; set; }
		public double ForecastWithShrinkage { get; set; }

		public double StaffingLevelWithShrinkage { get; set; }

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
			_loggedOn = loggedOn;
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
		public DateTimePeriod DateTimePeriod => new DateTimePeriod(StartDateTime.Utc(), EndDateTime.Utc());

		public double CalculatedResource	
		{
			get { return StaffingLevel; }
			set { StaffingLevel = value; }
		}

		public double FStaff
		{
			get { return Forecast; }
			set { Forecast = value; }
		}

		public void ClearIntraIntervalDistribution()
		{
			IntraIntervalDeviation = 0;
			IntraIntervalRootMeanSquare = 0;
		}

		public void SetDistributionValues(IPopulationStatisticsCalculatedValues calculatedValues,
			IPeriodDistribution periodDistribution)
		{
			IntraIntervalDeviation = calculatedValues.StandardDeviation;
			IntraIntervalRootMeanSquare = calculatedValues.RootMeanSquare;
		}

		public double IntraIntervalDeviation { get; private set; }
		public double IntraIntervalRootMeanSquare { get; private set; }
	}
}