using System;

namespace Teleopti.Ccc.Domain.Notification
{
	public class PersonScheduleChangeMessage
	{
		public Guid Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid PersonId { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}
