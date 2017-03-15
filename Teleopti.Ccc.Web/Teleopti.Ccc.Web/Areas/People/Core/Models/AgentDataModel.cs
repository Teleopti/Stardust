﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class AgentDataModel
	{
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string WindowsUser { get; set; }
		public string ApplicationUserId { get; set; }
		public string Password { get; set; }
		public List<IApplicationRole> Roles { get; set; }
		public DateOnly StartDate { get; set; }
		public ITeam Team { get; set; }
		public List<ISkill> Skills { get; set; }
		public IList<IExternalLogOn> ExternalLogons { get; set; }
		public IContract Contract { get; set; }
		public IContractSchedule ContractSchedule { get; set; }
		public IPartTimePercentage PartTimePercentage { get; set; }
		public IRuleSetBag RuleSetBag { get; set; }
		public SchedulePeriodType SchedulePeriodType { get; set; }
		public int SchedulePeriodLength { get; set; }
	}
}