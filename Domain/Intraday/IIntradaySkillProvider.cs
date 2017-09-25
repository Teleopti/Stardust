using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SkillGroup;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IIntradaySkillProvider
	{
		SkillGroup.SkillGroup GetSkillAreaById(Guid id);
		ISkill GetSkillById(Guid id);
		Guid[] GetSkillsFromSkillArea(Guid skillAreaId);
	}
}