using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IResourceCalculateAfterDeleteDecider
	{
		bool DoCalculation(IPerson agent, DateOnly date);
	}
}