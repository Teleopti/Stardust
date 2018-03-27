using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	[DisabledBy(Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
	public class OvertimeRequestCriticalUnderStaffedSpecification : Specification<OvertimeRequestValidatedSkillCount>,
		IOvertimeRequestCriticalUnderStaffedSpecification
	{
		public override bool IsSatisfiedBy(OvertimeRequestValidatedSkillCount obj)
		{
			return obj.CriticalUnderStaffedSkillCount == obj.SkillCount;
		}
	}

	[EnabledBy(Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
	public class OvertimeRequestCriticalUnderStaffedSpecificationToggle74944On :
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