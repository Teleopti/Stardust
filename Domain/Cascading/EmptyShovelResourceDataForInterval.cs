using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class EmptyShovelResourceDataForInterval : IShovelResourceDataForInterval
	{
		public void AddResources(double resourcesToAdd)
		{
		}

		public double AbsoluteDifference{ get; }

		public double CalculatedResource { get; }

		public double FStaff { get; }
	}
}