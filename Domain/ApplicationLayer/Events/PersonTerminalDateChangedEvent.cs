using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonTerminalDateChangedEvent : EventWithLogOnAndInitiator
	{
		public Guid PersonId { get; set; }
		public Guid? BusinessUnitId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid? TeamId { get; set; }
		public string TimeZoneInfoId { get; set; }
		public DateTime? PreviousTerminationDate { get; set; }
		public DateTime? TerminationDate { get; set; }
	}
}