using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class AlwaysResourceCalculateAfterDelete : IResourceCalculateAfterDeleteDecider
	{
		public bool DoCalculation(IPerson agent, DateOnly date)
		{
			return true;
		}
	}
}