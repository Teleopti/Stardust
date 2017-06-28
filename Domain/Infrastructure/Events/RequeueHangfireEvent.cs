using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Infrastructure.Events
{
	public class RequeueHangfireEvent : IEvent
	{
		public IList<string> Tenants { get; set; }
	}
}