using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeOvertimeRequestUnderStaffingSkillProvider : IOvertimeRequestUnderStaffingSkillProvider
	{
		readonly IList<ISkill> skills =new List<ISkill>();
		public void AddSeriousUnderstaffingSkill(ISkill skill)
		{
			skills.Add(skill);
		}

		public IList<ISkill> GetSeriousUnderstaffingSkills(DateTimePeriod period, IEnumerable<ISkill> skilllist)
		{
			return skills.Intersect(skilllist).ToList();
		}
	}
}
