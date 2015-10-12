using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class GroupPersonBuilderForOptimizationAndSingleAgentTeam : IGroupPersonBuilderForOptimization
	{
		public Group BuildGroup(IPerson person, DateOnly dateOnly)
		{
			return new Group(new [] { person }, person.Name.ToString());
		}
	}
}