using System.Collections.Generic;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ILoadAllSkillInIntradays
	{
		IEnumerable<SkillInIntraday> SkillsWithAtleastOneQueueSource();
		IEnumerable<SkillInIntraday> AllSkills();
	}
}