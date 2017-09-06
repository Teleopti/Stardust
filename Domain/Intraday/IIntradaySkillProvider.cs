using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IIntradaySkillProvider
	{
		SkillArea GetSkillAreaById(Guid id);
		ISkill GetSkillById(Guid id);
		Guid[] GetSkillsFromSkillArea(Guid skillAreaId);
	}
}