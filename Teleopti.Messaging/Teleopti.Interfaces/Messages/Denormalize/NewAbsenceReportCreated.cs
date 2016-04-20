using System;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	public class NewAbsenceReportCreated : MessageWithLogOnContext
	{
		public Guid AbsenceId { get; set; }
		public DateTime RequestedDate { get; set; }

		public override Guid Identity
		{
			get { return AbsenceId; }
		}

		public Guid PersonId { get; set; }
	}
}