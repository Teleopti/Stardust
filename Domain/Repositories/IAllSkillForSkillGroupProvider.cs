using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAllSkillForSkillGroupProvider
	{
		IEnumerable<SkillInIntraday> AllExceptSubSkills();
	}
}