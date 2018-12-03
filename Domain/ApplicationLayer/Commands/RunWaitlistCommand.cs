using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RunWaitlistCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public DateTimePeriod Period { get; set; }
		public IList<string> ErrorMessages { get; set; }

	    public Guid CommandId { get; set; }
	}
}