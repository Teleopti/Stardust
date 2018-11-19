using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PlanningGroupConfigurable : IDataSetup
	{
		public string Team { get; set; }
		public string PlanningGroupName { get; set; }

		public PlanningGroup PlanningGroup;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			PlanningGroup = new PlanningGroup();
			PlanningGroup.ChangeName(PlanningGroupName);
			if (Team != null)
			{
				var team = new TeamRepository(currentUnitOfWork).FindTeamByDescriptionName(Team).First();
				PlanningGroup.AddFilter(new TeamFilter(team));
			}
				
			new PlanningGroupRepository(currentUnitOfWork).Add(PlanningGroup);
		}
	}
}