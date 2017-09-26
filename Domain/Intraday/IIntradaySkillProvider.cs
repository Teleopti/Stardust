using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SkillGroupManagement;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IIntradaySkillProvider
	{
		SkillGroup GetSkillGroupById(Guid id);
		ISkill GetSkillById(Guid id);
		Guid[] GetSkillsFromSkillGroup(Guid skillGroupId);
	}
}