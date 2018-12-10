using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class AgentDataModel
	{
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string WindowsUser { get; set; }
		public string ApplicationUserId { get; set; }
		public string Password { get; set; }
		public List<IApplicationRole> Roles { get; private set; }
		public DateOnly StartDate { get; set; }
		public ITeam Team { get; set; }
		public List<ISkill> Skills { get; private set; }
		public List<IExternalLogOn> ExternalLogons { get; private set; }
		public IContract Contract { get; set; }
		public IContractSchedule ContractSchedule { get; set; }
		public IPartTimePercentage PartTimePercentage { get; set; }
		public IRuleSetBag RuleSetBag { get; set; }
		public SchedulePeriodType SchedulePeriodType { get; set; }
		public int SchedulePeriodLength { get; set; }

		public AgentDataModel()
		{
			Roles = new List<IApplicationRole>();
			Skills = new List<ISkill>();
			ExternalLogons = new List<IExternalLogOn>();
		}
	}
}