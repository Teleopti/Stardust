using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class OvertimeRequestCriticalUnderStaffedSpecification :
		Specification<OvertimeRequestValidatedSkillCount>, IOvertimeRequestCriticalUnderStaffedSpecification
	{
		public override bool IsSatisfiedBy(OvertimeRequestValidatedSkillCount obj)
		{
			return obj.CriticalUnderStaffedSkillCount > 0;
		}
	}

	public struct OvertimeRequestValidatedSkillCount
	{
		public int CriticalUnderStaffedSkillCount { get; }
		public int SkillCount { get; }

		public OvertimeRequestValidatedSkillCount(int criticalUnderStaffedSkillCount, int skillCount)
		{
			CriticalUnderStaffedSkillCount = criticalUnderStaffedSkillCount;
			SkillCount = skillCount;
		}
	}
}