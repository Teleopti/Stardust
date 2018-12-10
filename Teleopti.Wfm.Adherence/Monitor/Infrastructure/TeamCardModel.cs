using System;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public class TeamCardModel
	{
		public Guid BusinessUnitId { get; set; }
		public Guid SiteId { get; set; }
		public string SiteName { get; set; }
		public Guid TeamId { get; set; }
		public string TeamName { get; set; }
		public int InAlarmCount { get; set; }
		public int AgentsCount { get; set; }
	}
}