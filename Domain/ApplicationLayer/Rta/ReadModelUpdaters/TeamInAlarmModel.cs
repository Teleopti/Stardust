using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class TeamInAlarmModel
	{
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public int Count { get; set; }
	}
}