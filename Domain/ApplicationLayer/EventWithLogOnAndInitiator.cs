using System;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[Serializable]
	public class EventWithLogOnAndInitiator : Event, ILogOnInfo, IInitiatorInfo
	{
		public DateTime Timestamp { get; set; }
		public Guid InitiatorId { get; set; }
		public string LogOnDatasource { get; set; }
		public Guid LogOnBusinessUnitId { get; set; }
	}
}