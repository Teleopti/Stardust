using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Infrastructure.Events
{
	public class PublishInitializeReadModelEvent : IEvent
	{
		public IList<string> Tenants { get; set; }
		public int InitialLoadScheduleProjectionStartDate { get; set; }
		public int InitialLoadScheduleProjectionEndDate { get; set; }
	}
}