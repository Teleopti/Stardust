using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class SiteInAlarmModel
	{
		public Guid SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public int Count { get; set; }
	}
}