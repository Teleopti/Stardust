using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public class SiteOpenHoursCheckItem
	{
		public DateTimePeriod Period { get; set; }
		public IPerson Person { get; set; }
	}
}
