using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAllSkillForSkillGroupProvider
	{
		IEnumerable<SkillInIntraday> AllExceptSubSkills();
	}
}