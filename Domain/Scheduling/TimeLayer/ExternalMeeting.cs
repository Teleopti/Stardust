using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
	public class ExternalMeeting : AggregateRoot
	{
		public virtual string Title { get; set; }
		public virtual string Agenda { get; set; }
	}
}	