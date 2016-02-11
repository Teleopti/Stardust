using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface ISkillGroupInfo
	{
		bool ResourceCalculateAfterDelete(IPerson agent, DateOnly date);
	}
}