using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class CancelAbsenceRequestCommand : ITrackableCommand, IErrorAttachedCommand, IReplyCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public Guid PersonRequestId { get; set; }
		public Guid? AffectedRequestId { get; set; }
		public IList<string> ErrorMessages { get; set; }
		public string ReplyMessage { get; set; }
		public bool IsReplySuccess { get; set; }
	}

	public interface ICancelAbsenceRequestCommandProvider
	{
		CancelAbsenceRequestCommand CancelAbsenceRequest(Guid personRequestId);

	}

}