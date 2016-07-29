using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class DenyRequestCommand : ITrackableCommand, IErrorAttachedCommand, IReplyCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public Guid PersonRequestId { get; set; }
		public Guid? AffectedRequestId { get; set; }
		public bool IsManualDeny { get; set; }
		public IList<string> ErrorMessages { get; set; }
		public string ReplyMessage { get; set; }
		public bool IsReplySuccess { get; set; }
	}
}
