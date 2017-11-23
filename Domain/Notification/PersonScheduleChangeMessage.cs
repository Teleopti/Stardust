using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Notification
{
	public class PersonScheduleChangeMessage : AggregateRoot
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid PersonId { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}
