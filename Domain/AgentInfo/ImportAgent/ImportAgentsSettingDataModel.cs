using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.ImportAgent
{
	public class ImportAgentsFieldOptionsModel
	{
		public IDictionary<Guid, string> Roles { get; set; }
		public IDictionary<Guid, string> Teams { get; set; }
		public IDictionary<Guid, string> Skills { get; set; }
		public IDictionary<Guid, string> Contracts { get; set; }
		public IDictionary<Guid, string> ContractSchedules { get; set; }
		public IDictionary<Guid, string> ShiftBags { get; set; }
		public IDictionary<int, string> SchedulePeriodTypes { get; set; }
		public IDictionary<Guid, string> PartTimePercentages { get; set; }
		public IDictionary<Guid, string> ExternalLogons { get; set; }
	}

	public class TeamViewModel
	{
		public string Id { get; set; }
		public string SiteAndTeam { get; set; }
	}
}