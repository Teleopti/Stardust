using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentsFieldOptionsModel
	{
		public IList<FieldOptionViewModel> Roles { get; set; }
		public IDictionary<Guid, string> Teams { get; set; }
		public IList<FieldOptionViewModel> Skills { get; set; }
		public IList<FieldOptionViewModel> Contracts { get; set; }
		public IList<FieldOptionViewModel> ContractSchedules { get; set; }
		public IList<FieldOptionViewModel> ShiftBags { get; set; }
		public IDictionary<int, string> SchedulePeriodTypes { get; set; }
		public IList<FieldOptionViewModel> PartTimePercentages { get; set; }
		public IList<FieldOptionViewModel> ExternalLogons { get; set; }
	}

	public class FieldOptionViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}

	public class TeamViewModel
	{
		public string Id { get; set; }
		public string SiteAndTeam { get; set; }
	}
}