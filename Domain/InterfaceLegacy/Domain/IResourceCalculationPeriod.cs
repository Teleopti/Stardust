using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IResourceCalculationPeriod
	{
		double ForecastedDistributedDemand { get; }
		void SetCalculatedResource65(double resources);
		void SetCalculatedLoggedOn(double loggedOn);
		void ResetMultiskillMinOccupancy();

		DateTimePeriod CalculationPeriod { get; }
		double CalculatedLoggedOn { get;  }
		void SetCalculatedUsedSeats(double usedSeats);
		double FStaff { get; }
		void ClearIntraIntervalDistribution();
		void SetDistributionValues(IPopulationStatisticsCalculatedValues calculatedValues, IPeriodDistribution periodDistribution);
		double IntraIntervalDeviation { get; }
		double IntraIntervalRootMeanSquare { get; }
	}
}