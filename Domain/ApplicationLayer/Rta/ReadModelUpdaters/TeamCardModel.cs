using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class TeamCardModel
	{
		public Guid BusinessUnitId { get; set; }
		public Guid SiteId { get; set; }
		public String SiteName { get; set; }
		public Guid TeamId { get; set; }
		public String TeamName { get; set; }
		public int InAlarmCount { get; set; }
		public int AgentsCount { get; set; }
	}
}