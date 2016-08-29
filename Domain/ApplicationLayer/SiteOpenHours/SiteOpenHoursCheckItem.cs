using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public class SiteOpenHoursCheckItem
	{
		public DateTimePeriod Period { get; set; }
		public IPerson Person { get; set; }
	}
}
