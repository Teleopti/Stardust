using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class DontConsiderSkillGroupInfo : ISkillGroupInfo
	{
		public bool ResourceCalculateAfterDelete(IPerson agent, DateOnly date)
		{
			return true;
		}
	}
}