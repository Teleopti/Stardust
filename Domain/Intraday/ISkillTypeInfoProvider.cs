using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface ISkillTypeInfoProvider
	{
		SkillTypeInfo GetSkillTypeInfo(ISkill skill);
	}
}