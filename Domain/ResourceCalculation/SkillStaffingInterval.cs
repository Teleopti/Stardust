using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillStaffingInterval : IOvertimeSkillPeriodData, IValidatePeriod
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		
		public DateTimePeriod CalculationPeriod => new DateTimePeriod(StartDateTime, EndDateTime);
		public double CalculatedLoggedOn { get; private set; }

		public Percent EstimatedServiceLevel { get; set; }

		public double StaffingLevelWithShrinkage { get; set; }

		public void SetCalculatedUsedSeats(double usedSeats)
		{
		}

		public double Forecast { get; set; }
		public double StaffingLevel { get; set; }

		public TimeSpan GetTimeSpan()
		{
			return EndDateTime.Subtract(StartDateTime);
		}

		public double DivideBy(TimeSpan ts)
		{
			return (double) GetTimeSpan().Ticks / ts.Ticks;
		}

		public double ForecastedDistributedDemand => Forecast;

		public void SetCalculatedResource65(double resources)
		{
			StaffingLevel = resources;
		}

		public void SetCalculatedLoggedOn(double loggedOn)
		{
			CalculatedLoggedOn = loggedOn;
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
			get => StaffingLevel;
			set => StaffingLevel = value;
		}

		public double FStaff
		{
			get => Forecast;
			set => Forecast = value;
		}

		public void ClearIntraIntervalDistribution()
		{
			IntraIntervalDeviation = 0;
			IntraIntervalRootMeanSquare = 0;
		}

		public void SetDistributionValues(PopulationStatisticsCalculatedValues calculatedValues,
										  IPeriodDistribution periodDistribution)
		{
			IntraIntervalDeviation = calculatedValues.StandardDeviation;
			IntraIntervalRootMeanSquare = calculatedValues.RootMeanSquare;
		}

		public double IntraIntervalDeviation { get; private set; }
		public double IntraIntervalRootMeanSquare { get; private set; }
		public Percent Shrinkage { get; set; }

		public void SetUseShrinkage(bool setValue)
		{
			if (setValue)
			{
				if (Shrinkage.Value >= 1.0)
					FStaff = 0;
				else
				{
					FStaff = ForecastWithoutShrinkage / (1 - Shrinkage.Value);
				}
			}
			else
				FStaff = ForecastWithoutShrinkage;
		}

		public double ForecastWithoutShrinkage { get; set; }
		
		public bool IsBacklogType { get; set; }
	}
}