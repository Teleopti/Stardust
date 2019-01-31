using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[Serializable]
	public class EventWithInfrastructureContext : Event, ITimestamped, ILogOnContext, IInitiatorContext
	{
		public DateTime Timestamp { get; set; }
		public Guid InitiatorId { get; set; }
		public string LogOnDatasource { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
	}
}