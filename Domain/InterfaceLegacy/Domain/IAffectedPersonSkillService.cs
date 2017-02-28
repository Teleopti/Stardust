using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAffectedPersonSkillService
	{
		IEnumerable<ISkill> AffectedSkills { get; }
	}
}
