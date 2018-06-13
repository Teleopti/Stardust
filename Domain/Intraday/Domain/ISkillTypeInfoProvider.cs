using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public interface ISkillTypeInfoProvider
	{
		SkillTypeInfo GetSkillTypeInfo(ISkill skill);
	}
}