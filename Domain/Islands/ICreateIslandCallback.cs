using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public interface ICreateIslandsCallback
	{
		void BasicIslandsCreated(IEnumerable<IEnumerable<SkillSet>> basicIslands, IDictionary<ISkill, int> noAgentsKnowingSkill);
		void AfterExtendingDueToReducing(IEnumerable<Island> islands);
		void Complete(IEnumerable<Island> islands);
	}
}