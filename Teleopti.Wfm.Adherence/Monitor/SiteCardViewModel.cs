using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Monitor
{
	public class SiteCardViewModel
	{
		public int TotalAgentsInAlarm { get; set; }
		public IEnumerable<SiteViewModel> Sites { get; set; }
	}

	public class SiteViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int AgentsCount { get; set; }
		public int InAlarmCount { get; set; }
		public string Color { get; set; }
		public IEnumerable<TeamCardViewModel> Teams { get; set; }
	}
}