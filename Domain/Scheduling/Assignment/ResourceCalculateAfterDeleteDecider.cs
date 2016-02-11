using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ResourceCalculateAfterDeleteDecider : IResourceCalculateAfterDeleteDecider
	{
		public bool DoCalculation(IPerson agent, DateOnly date)
		{ 
			return true;
		}
	}
}