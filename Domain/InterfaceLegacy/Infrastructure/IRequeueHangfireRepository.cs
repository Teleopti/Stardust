using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IRequeueHangfireRepository
	{
		IList<RequeueCommand> GetUnhandledRequeueCommands();
		void MarkAsCompleted(RequeueCommand command);
	}

	public class RequeueCommand
	{
		public virtual Guid Id { get; set; }
		public virtual string EventName { get; set; }
		public virtual string HandlerName { get; set; }
		public virtual bool Handled { get; set; }
		public virtual DateTime Timestamp { get; set; }
	}
}