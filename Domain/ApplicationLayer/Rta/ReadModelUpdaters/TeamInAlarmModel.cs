using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class TeamInAlarmModel
	{
		public Guid BusinessUnitId { get; set; }
		public Guid SiteId { get; set; }
		public Guid TeamId { get; set; }
		public int InAlarmCount { get; set; }
	}
}