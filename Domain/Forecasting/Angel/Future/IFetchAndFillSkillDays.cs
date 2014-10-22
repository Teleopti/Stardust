using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public interface IFetchAndFillSkillDays
	{
		ICollection<ISkillDay> FindRange(DateOnlyPeriod futurePeriod, ISkill skill);
	}
}