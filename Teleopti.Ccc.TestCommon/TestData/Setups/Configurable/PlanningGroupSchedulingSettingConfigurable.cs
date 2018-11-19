using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PlanningGroupSchedulingSettingConfigurable : IDataSetup
	{
		public string Team { get; set; }
		public string SchedulingSettingName { get; set; }
		public string PlanningGroupName { get; set; }
		public string BlockScheduling { get; set; }
		public PlanningGroup PlanningGroup;
		public PlanningGroupSettings PlanningGroupSchedulingSetting;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			ITeam team = new Team();

			if (Team != null)
			{
				team = new TeamRepository(currentUnitOfWork).FindTeamByDescriptionName(Team).First();
			}

			PlanningGroup = new PlanningGroup {Name = SchedulingSettingName};
			PlanningGroup.AddFilter(new TeamFilter(team));

			PlanningGroupSchedulingSetting = new PlanningGroupSettings {Name = SchedulingSettingName};
			PlanningGroupSchedulingSetting.AddFilter(new TeamFilter(team));
			if (BlockScheduling == "default")
			{
				PlanningGroupSchedulingSetting.BlockSameShiftCategory = true;
				PlanningGroupSchedulingSetting.BlockSameShift = false;
				PlanningGroupSchedulingSetting.BlockSameStartTime = false;
				PlanningGroupSchedulingSetting.BlockFinderType = BlockFinderType.BetweenDayOff;
			}
			PlanningGroup.AddSetting(PlanningGroupSchedulingSetting);
			new PlanningGroupRepository(currentUnitOfWork).Add(PlanningGroup);
		}
	}
}