using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[Serializable]
	public abstract class EventWithLogOnAndInitiator : Event, ILogOnInfo, IInitiatorInfo
	{
		public DateTime Timestamp { get; set; }
		public Guid InitiatorId { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}