using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IRequestCommand : ITrackableCommand, IReplyCommand, IErrorAttachedCommand
	{
		Guid? AffectedRequestId { get; set; }
	}
}