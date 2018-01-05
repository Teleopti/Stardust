using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class NullCreateIslandsCallback : ICreateIslandsCallback
	{
		public void BasicIslandsCreated(IEnumerable<IEnumerable<SkillSet>> basicIslands,
			IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
		}

		public void AfterExtendingDueToReducing(IEnumerable<Island> islands)
		{
		}
	}
}