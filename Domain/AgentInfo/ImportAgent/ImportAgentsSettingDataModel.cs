using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.ImportAgent
{
	public class ImportAgentSettingsDataModel
	{
		public List<IApplicationRole> Roles { get; set; }
		public DateOnly StartDate { get; set; }
		public List<ITeam> Teams { get; set; }
		public List<ISkill> Skills { get; set; }
		public List<IExternalLogOn> ExternalLogons { get; set; }
		public List<IContract> Contracts { get; set; }
		public List<IContractSchedule> ContractSchedules { get; set; }
		public List<IPartTimePercentage> PartTimePercentages { get; set; }
		public List<IRuleSetBag> ShiftBags { get; set; }
		public List<SchedulePeriodType> SchedulePeriodTypes { get; set; }
		public int SchedulePeriodLength { get; set; }
	}
}