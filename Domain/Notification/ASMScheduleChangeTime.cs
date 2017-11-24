using System;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ASMScheduleChangeTime
	{
		public Guid PersonId { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}
