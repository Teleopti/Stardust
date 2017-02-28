using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAffectedPersonSkillService
	{
		IEnumerable<ISkill> AffectedSkills { get; }
	}
}
