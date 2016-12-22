namespace Teleopti.Interfaces.Domain
{
	public interface IResourceCalculationPeriod
	{
		double ForecastedDistributedDemand { get; }
		void SetCalculatedResource65(double resources);
		void SetCalculatedLoggedOn(double loggedOn);
		void ResetMultiskillMinOccupancy();
	}
}