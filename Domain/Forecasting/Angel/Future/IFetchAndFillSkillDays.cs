using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public interface IFetchAndFillSkillDays
	{
		ICollection<ISkillDay> FindRange(DateOnlyPeriod futurePeriod, ISkill skill);
		ICollection<ISkillDay> FindRange(DateOnlyPeriod futurePeriod, ISkill skill, IScenario scenario);
	}
}