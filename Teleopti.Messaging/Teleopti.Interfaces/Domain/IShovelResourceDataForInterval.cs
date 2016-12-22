namespace Teleopti.Interfaces.Domain
{
	public interface IShovelResourceDataForInterval
	{
		void AddResources(double resourcesToAdd);
		double AbsoluteDifference { get; }
		double CalculatedResource { get; }
		double RelativeDifference { get; }
		double FStaff { get; }
	}
}