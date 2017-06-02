using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class AgentGroupConfigurable : IDataSetup
	{
		public string Team { get; set; }
		public string AgentGroupName { get; set; }


		public IAgentGroup AgentGroup;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			AgentGroup = new AgentGroup(AgentGroupName);
			if (Team != null)
			{
				var team = new TeamRepository(currentUnitOfWork).FindTeamByDescriptionName(Team).First();
				AgentGroup.AddFilter(new TeamFilter(team));
			}
				
			new AgentGroupRepository(currentUnitOfWork).Add(AgentGroup);
		}
	}
}