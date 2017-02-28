using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[Serializable]
	public class EventWithInfrastructureContext : Event, ITimestamped, ILogOnContext, IInitiatorContext, IStardustJobInfo
	{
		public DateTime Timestamp { get; set; }
		public Guid InitiatorId { get; set; }
		public string LogOnDatasource { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
		public string JobName { get; set; }
		public string UserName { get; set; }
	}

	[Serializable]
	public class EventWithLogOnContext : Event, ILogOnContext
	{
		public string LogOnDatasource { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
	}

}